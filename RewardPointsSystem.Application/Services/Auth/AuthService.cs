using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RewardPointsSystem.Application.Configuration;
using RewardPointsSystem.Application.DTOs.Auth;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Auth
{
    /// <summary>
    /// Service implementation for authentication and authorization operations.
    /// Contains all authentication business logic - Clean Architecture compliant.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userService = userService;
            _roleService = roleService;
            _userRoleService = userRoleService;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<AuthResult<LoginResponseDto>> RegisterAsync(RegisterRequestDto dto, string? clientIp)
        {
            try
            {
                // Validate passwords match
                if (dto.Password != dto.ConfirmPassword)
                {
                    return AuthResult<LoginResponseDto>.Failed("Passwords do not match", AuthErrorType.ValidationError);
                }

                // Validate password strength
                var passwordValidation = ValidatePasswordStrength(dto.Password);
                if (!passwordValidation.Success)
                {
                    return AuthResult<LoginResponseDto>.Failed(passwordValidation.ErrorMessage!, AuthErrorType.ValidationError);
                }

                // Check if user already exists
                var existingUser = await _userService.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return AuthResult<LoginResponseDto>.Failed($"User with email {dto.Email} already exists", AuthErrorType.Conflict);
                }

                // Create user
                var user = await _userService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName);

                // Hash and set password
                var passwordHash = _passwordHasher.HashPassword(dto.Password);
                user.SetPasswordHash(passwordHash);

                // Update user to persist password hash
                var updateDto = new UserUpdateDto
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                await _userService.UpdateUserAsync(user.Id, updateDto);

                // Assign default "Employee" role
                var employeeRole = await _roleService.GetRoleByNameAsync("Employee");
                if (employeeRole != null)
                {
                    await _userRoleService.AssignRoleAsync(user.Id, employeeRole.Id, user.Id);
                }

                // Generate tokens
                var (accessToken, refreshToken, expiresAt) = await GenerateTokensAsync(user.Id, clientIp);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    Roles = employeeRole != null ? new[] { employeeRole.Name } : Array.Empty<string>()
                };

                _logger.LogInformation("User {Email} registered successfully", user.Email);
                return AuthResult<LoginResponseDto>.Succeeded(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email {Email}", dto.Email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AuthResult<LoginResponseDto>> LoginAsync(LoginRequestDto dto, string? clientIp)
        {
            try
            {
                // Get user by email
                var user = await _userService.GetUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    return AuthResult<LoginResponseDto>.Failed("Invalid email or password", AuthErrorType.BadRequest);
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return AuthResult<LoginResponseDto>.Failed("User account is inactive", AuthErrorType.BadRequest);
                }

                // Verify password
                if (!user.HasPassword() || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash!))
                {
                    return AuthResult<LoginResponseDto>.Failed("Invalid email or password", AuthErrorType.BadRequest);
                }

                // Generate tokens
                var (accessToken, refreshToken, expiresAt) = await GenerateTokensAsync(user.Id, clientIp);

                // Get user roles for response
                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    Roles = roles.Select(r => r.Name).ToArray()
                };

                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return AuthResult<LoginResponseDto>.Succeeded(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", dto.Email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(string refreshToken, string? clientIp)
        {
            try
            {
                // Validate refresh token
                var userId = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
                if (userId == null)
                {
                    return AuthResult<TokenResponseDto>.Failed("Invalid or expired refresh token", AuthErrorType.Unauthorized);
                }

                // Get user and verify active
                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null || !user.IsActive)
                {
                    return AuthResult<TokenResponseDto>.Failed("User not found or inactive", AuthErrorType.Unauthorized);
                }

                // Get user roles
                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Store new refresh token
                var jwtSettings = GetJwtSettings();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);
                await _tokenService.StoreRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry, clientIp);

                // Revoke old refresh token
                await _tokenService.RevokeRefreshTokenAsync(refreshToken, clientIp, "Replaced by new token", newRefreshToken);

                var response = new TokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes)
                };

                _logger.LogInformation("Token refreshed for user {UserId}", user.Id);
                return AuthResult<TokenResponseDto>.Succeeded(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> LogoutAsync(Guid userId, string? clientIp)
        {
            var revokedCount = await _tokenService.RevokeAllUserRefreshTokensAsync(userId, clientIp, "User logout");
            _logger.LogInformation("User {UserId} logged out, {Count} tokens revoked", userId, revokedCount);
            return revokedCount;
        }

        /// <inheritdoc />
        public async Task<AuthResult<UserInfoDto>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return AuthResult<UserInfoDto>.Failed("User not found", AuthErrorType.NotFound);
                }

                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                var userInfo = new UserInfoDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.Select(r => r.Name).ToArray()
                };

                return AuthResult<UserInfoDto>.Succeeded(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AuthResult> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(dto.CurrentPassword) || string.IsNullOrEmpty(dto.NewPassword))
                {
                    return AuthResult.Failed("Current password and new password are required", AuthErrorType.ValidationError);
                }

                // Validate new password strength
                var passwordValidation = ValidatePasswordStrength(dto.NewPassword);
                if (!passwordValidation.Success)
                {
                    return AuthResult.Failed(passwordValidation.ErrorMessage!, AuthErrorType.ValidationError);
                }

                // Get user
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return AuthResult.Failed("User not found", AuthErrorType.NotFound);
                }

                // Verify current password
                if (!user.HasPassword() || !_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash!))
                {
                    return AuthResult.Failed("Current password is incorrect", AuthErrorType.BadRequest);
                }

                // Hash and set new password
                var newPasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
                user.SetPasswordHash(newPasswordHash);

                // Update user to persist new password hash
                var updateDto = new UserUpdateDto
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };
                await _userService.UpdateUserAsync(user.Id, updateDto);

                _logger.LogInformation("User {UserId} changed their password", userId);
                return AuthResult.Succeeded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Generates access and refresh tokens for a user.
        /// </summary>
        private async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> GenerateTokensAsync(Guid userId, string? clientIp)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            var roles = await _userRoleService.GetUserRolesAsync(userId);

            var jwtSettings = GetJwtSettings();

            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);
            await _tokenService.StoreRefreshTokenAsync(userId, refreshToken, refreshTokenExpiry, clientIp);

            var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes);

            return (accessToken, refreshToken, expiresAt);
        }

        /// <summary>
        /// Gets JWT settings.
        /// </summary>
        private JwtSettings GetJwtSettings()
        {
            return _jwtSettings;
        }

        /// <summary>
        /// Validates password strength requirements.
        /// </summary>
        private static AuthResult ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return AuthResult.Failed("Password is required", AuthErrorType.ValidationError);
            }

            if (password.Length < 8)
            {
                return AuthResult.Failed("Password must be at least 8 characters long", AuthErrorType.ValidationError);
            }

            // Additional password strength rules can be added here:
            // - Require uppercase
            // - Require lowercase
            // - Require numbers
            // - Require special characters

            return AuthResult.Succeeded();
        }

        #endregion
    }
}
