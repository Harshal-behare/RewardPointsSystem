namespace RewardPointsSystem.Application.Configuration
{
    /// <summary>
    /// JWT authentication settings configuration
    /// Maps to appsettings.json "JwtSettings" section
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Secret key for signing JWT tokens (minimum 256 bits / 32 characters)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Token issuer (typically the API name)
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Token audience (typically the client/consumer name)
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Access token expiration time in minutes (recommended: 15-60 minutes)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh token expiration time in days (recommended: 7-30 days)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Validates the JWT settings configuration
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SecretKey))
                throw new InvalidOperationException("JWT SecretKey is not configured.");

            if (SecretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey must be at least 32 characters (256 bits) for security.");

            if (string.IsNullOrWhiteSpace(Issuer))
                throw new InvalidOperationException("JWT Issuer is not configured.");

            if (string.IsNullOrWhiteSpace(Audience))
                throw new InvalidOperationException("JWT Audience is not configured.");

            if (AccessTokenExpirationMinutes < 1)
                throw new InvalidOperationException("JWT AccessTokenExpirationMinutes must be at least 1.");

            if (RefreshTokenExpirationDays < 1)
                throw new InvalidOperationException("JWT RefreshTokenExpirationDays must be at least 1.");
        }
    }
}
