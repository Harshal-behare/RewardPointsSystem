using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Auth;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages authentication and authorization operations.
    /// Clean Architecture compliant - delegates all business logic to IAuthService.
    /// </summary>
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
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
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.RegisterAsync(dto, clientIp);

                if (!result.Success)
                {
                    return MapAuthErrorToResponse(result.ErrorType, result.ErrorMessage!);
                }

                return Created(result.Data!, "User registered successfully");
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
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.LoginAsync(dto, clientIp);

                if (!result.Success)
                {
                    return MapAuthErrorToResponse(result.ErrorType, result.ErrorMessage!);
                }

                return Success(result.Data!, "Login successful");
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
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.RefreshTokenAsync(dto.RefreshToken, clientIp);

                if (!result.Success)
                {
                    return MapAuthErrorToResponse(result.ErrorType, result.ErrorMessage!);
                }

                return Success(result.Data!, "Token refreshed successfully");
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
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return UnauthorizedError("User not authenticated");

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                await _authService.LogoutAsync(userId.Value, clientIp);

                return Success<object>(null, "Logout successful");
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedError("Invalid user");
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
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return UnauthorizedError("User not authenticated");

                var result = await _authService.GetCurrentUserAsync(userId.Value);

                if (!result.Success)
                {
                    return MapAuthErrorToResponse(result.ErrorType, result.ErrorMessage!);
                }

                // Map UserInfoDto to LoginResponseDto for backward compatibility
                var response = new LoginResponseDto
                {
                    UserId = result.Data!.UserId,
                    Email = result.Data.Email,
                    FirstName = result.Data.FirstName,
                    LastName = result.Data.LastName,
                    AccessToken = string.Empty,
                    RefreshToken = string.Empty,
                    ExpiresAt = DateTime.UtcNow,
                    Roles = result.Data.Roles
                };

                return Success(response);
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedError("Invalid user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return Error("Failed to get user information");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="dto">Current and new password</param>
        /// <response code="200">Password changed successfully</response>
        /// <response code="400">Invalid current password or validation failed</response>
        /// <response code="401">User not authenticated</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                    return UnauthorizedError("User not authenticated");

                var result = await _authService.ChangePasswordAsync(userId.Value, dto);

                if (!result.Success)
                {
                    return MapAuthErrorToResponse(result.ErrorType, result.ErrorMessage!);
                }

                return Success<object>(null, "Password changed successfully");
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedError("Invalid user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return Error("Failed to change password");;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Maps AuthErrorType to appropriate HTTP response.
        /// </summary>
        private IActionResult MapAuthErrorToResponse(AuthErrorType errorType, string message)
        {
            return errorType switch
            {
                AuthErrorType.Unauthorized => UnauthorizedError(message),
                AuthErrorType.NotFound => NotFoundError(message),
                AuthErrorType.Conflict => ConflictError(message),
                AuthErrorType.ValidationError => Error(message, 422),
                _ => Error(message, 400)
            };
        }

        #endregion
    }
}
