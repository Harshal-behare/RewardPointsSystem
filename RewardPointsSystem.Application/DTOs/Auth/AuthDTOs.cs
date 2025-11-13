using System;

namespace RewardPointsSystem.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for user login request
    /// </summary>
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// DTO for user login response
    /// </summary>
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string[] Roles { get; set; }
    }

    /// <summary>
    /// DTO for user registration
    /// </summary>
    public class RegisterRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// DTO for refreshing access token
    /// </summary>
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// DTO for token response
    /// </summary>
    public class TokenResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
