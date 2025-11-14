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
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
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

                // In a real implementation, you would:
                // 1. Hash and store the password
                // 2. Generate JWT tokens
                // 3. Return tokens with user info

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = "jwt-token-placeholder", // TODO: Implement JWT generation
                    RefreshToken = "refresh-token-placeholder",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    Roles = new[] { "Employee" }
                };

                return Created(response, "User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
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

                // In a real implementation, you would:
                // 1. Verify password hash
                // 2. Generate JWT tokens
                // 3. Store refresh token

                var response = new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    AccessToken = "jwt-token-placeholder", // TODO: Implement JWT generation
                    RefreshToken = "refresh-token-placeholder",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    Roles = new[] { "Employee" } // TODO: Get actual user roles
                };

                return Success(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
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
                // In a real implementation, you would:
                // 1. Validate refresh token
                // 2. Check if token is not revoked
                // 3. Generate new access token
                // 4. Generate new refresh token
                // 5. Revoke old refresh token

                var response = new TokenResponseDto
                {
                    AccessToken = "new-jwt-token-placeholder",
                    RefreshToken = "new-refresh-token-placeholder",
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };

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
                // In a real implementation, you would:
                // 1. Revoke refresh token
                // 2. Optionally blacklist access token

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
                // In a real implementation, you would:
                // 1. Get user ID from JWT claims
                // 2. Fetch user from database
                // 3. Return user info

                // For now, return a placeholder
                return Success<object>(new { message = "Current user endpoint - JWT implementation pending" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return Error("Failed to get user information");
            }
        }
    }
}
