using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Auth;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages authentication and authorization operations
    /// </summary>
    public class AuthController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IUserRoleService _userRoleService;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUserService userService,
            IRoleService roleService,
            IUserRoleService userRoleService,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _userService = userService;
            _roleService = roleService;
            _userRoleService = userRoleService;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="dto">Registration data</param>
        /// <response code="201">User registered successfully</response>
        /// <response code="400">Invalid registration data</response>
        /// <response code="409">Email already exists</response>
        /// <response code="422">Validation failed</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                // Check if passwords match
                if (dto.Password != dto.ConfirmPassword)
                    return Error("Passwords do not match", 400);

                // Check if user already exists
                var existingUser = await _userService.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                    return ConflictError($"User with email {dto.Email} already exists");

                // Create user
                var user = await _userService.CreateUserAsync(dto.Email, dto.FirstName, dto.LastName);

                // Hash and set password
                var passwordHash = _passwordHasher.HashPassword(dto.Password);
                user.SetPasswordHash(passwordHash);
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

                // Get user roles for token generation
                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                // Generate JWT tokens
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Store refresh token
                var jwtSettings = _configuration.GetSection("JwtSettings").Get<Application.Configuration.JwtSettings>();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry, clientIp);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
                    Roles = roles.Select(r => r.Name).ToArray()
                };

                _logger.LogInformation("User {Email} registered successfully", user.Email);
                return Created(response, "User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email {Email}", dto.Email);
                return Error("Failed to register user");
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="dto">Login credentials</param>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid credentials</response>
        /// <response code="422">Validation failed</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                // Get user by email
                var user = await _userService.GetUserByEmailAsync(dto.Email);
                if (user == null)
                    return Error("Invalid email or password", 400);

                // Check if user is active
                if (!user.IsActive)
                    return Error("User account is inactive", 400);

                // Verify password
                if (!user.HasPassword() || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash!))
                    return Error("Invalid email or password", 400);

                // Get user roles for token generation
                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                // Generate JWT tokens
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Store refresh token
                var jwtSettings = _configuration.GetSection("JwtSettings").Get<Application.Configuration.JwtSettings>();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry, clientIp);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes),
                    Roles = roles.Select(r => r.Name).ToArray()
                };

                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", dto.Email);
                return Error("Login failed");
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="dto">Refresh token</param>
        /// <response code="200">Token refreshed successfully</response>
        /// <response code="401">Invalid refresh token</response>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                // Validate refresh token
                var userId = await _tokenService.ValidateRefreshTokenAsync(dto.RefreshToken);
                if (userId == null)
                    return UnauthorizedError("Invalid or expired refresh token");

                // Get user
                var user = await _userService.GetUserByIdAsync(userId.Value);
                if (user == null || !user.IsActive)
                    return UnauthorizedError("User not found or inactive");

                // Get user roles
                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Store new refresh token
                var jwtSettings = _configuration.GetSection("JwtSettings").Get<Application.Configuration.JwtSettings>();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(jwtSettings.RefreshTokenExpirationDays);
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _tokenService.StoreRefreshTokenAsync(user.Id, newRefreshToken, refreshTokenExpiry, clientIp);

                // Revoke old refresh token
                await _tokenService.RevokeRefreshTokenAsync(dto.RefreshToken, clientIp, "Replaced by new token", newRefreshToken);

                var response = new TokenResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.AccessTokenExpirationMinutes)
                };

                _logger.LogInformation("Token refreshed for user {UserId}", user.Id);
                return Success(response, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return UnauthorizedError("Invalid refresh token");
            }
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <response code="200">Logout successful</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return UnauthorizedError("Invalid user");

                // Revoke all refresh tokens for the user
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var revokedCount = await _tokenService.RevokeAllUserRefreshTokensAsync(userId, clientIp, "User logout");

                _logger.LogInformation("User {UserId} logged out, {Count} tokens revoked", userId, revokedCount);
                return Success<object>(null, "Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return Error("Logout failed");
            }
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        /// <response code="200">Returns current user information</response>
        /// <response code="401">User not authenticated</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Get user ID from JWT claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return UnauthorizedError("Invalid user");

                // Fetch user from database
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                    return NotFoundError("User not found");

                // Get user roles
                var roles = await _userRoleService.GetUserRolesAsync(user.Id);

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = string.Empty, // Not returning new token
                    RefreshToken = string.Empty, // Not returning new token
                    ExpiresAt = DateTime.UtcNow,
                    Roles = roles.Select(r => r.Name).ToArray()
                };

                return Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return Error("Failed to get user information");
            }
        }
    }
}
