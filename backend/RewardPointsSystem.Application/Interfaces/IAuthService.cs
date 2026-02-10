using RewardPointsSystem.Application.DTOs.Auth;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service interface for authentication and authorization operations.
    /// Encapsulates all authentication business logic.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Register a new user with the provided credentials.
        /// </summary>
        /// <param name="dto">Registration request data</param>
        /// <param name="clientIp">Client IP address for token tracking</param>
        /// <returns>Login response with user info and tokens</returns>
        Task<AuthResult<LoginResponseDto>> RegisterAsync(RegisterRequestDto dto, string? clientIp);

        /// <summary>
        /// Authenticate a user with email and password.
        /// </summary>
        /// <param name="dto">Login request data</param>
        /// <param name="clientIp">Client IP address for token tracking</param>
        /// <returns>Login response with user info and tokens</returns>
        Task<AuthResult<LoginResponseDto>> LoginAsync(LoginRequestDto dto, string? clientIp);

        /// <summary>
        /// Refresh an access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="clientIp">Client IP address for token tracking</param>
        /// <returns>New access and refresh tokens</returns>
        Task<AuthResult<TokenResponseDto>> RefreshTokenAsync(string refreshToken, string? clientIp);

        /// <summary>
        /// Logout user by revoking all refresh tokens.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="clientIp">Client IP address for logging</param>
        /// <returns>Number of tokens revoked</returns>
        Task<int> LogoutAsync(Guid userId, string? clientIp);

        /// <summary>
        /// Get current user information.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User information DTO</returns>
        Task<AuthResult<UserInfoDto>> GetCurrentUserAsync(Guid userId);

        /// <summary>
        /// Change user's password.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dto">Password change request</param>
        /// <returns>Success or failure result</returns>
        Task<AuthResult> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto);
    }

    /// <summary>
    /// Result wrapper for authentication operations
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public AuthErrorType ErrorType { get; set; }

        public static AuthResult Succeeded() => new() { Success = true };
        public static AuthResult Failed(string message, AuthErrorType errorType = AuthErrorType.BadRequest) 
            => new() { Success = false, ErrorMessage = message, ErrorType = errorType };
    }

    /// <summary>
    /// Generic result wrapper for authentication operations
    /// </summary>
    /// <typeparam name="T">The result data type</typeparam>
    public class AuthResult<T> : AuthResult
    {
        public T? Data { get; set; }

        public static AuthResult<T> Succeeded(T data) => new() { Success = true, Data = data };
        public new static AuthResult<T> Failed(string message, AuthErrorType errorType = AuthErrorType.BadRequest) 
            => new() { Success = false, ErrorMessage = message, ErrorType = errorType };
    }

    /// <summary>
    /// Authentication error types for proper HTTP status code mapping
    /// </summary>
    public enum AuthErrorType
    {
        BadRequest,
        Unauthorized,
        NotFound,
        Conflict,
        ValidationError
    }

    /// <summary>
    /// DTO for user information response (without tokens)
    /// </summary>
    public class UserInfoDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
