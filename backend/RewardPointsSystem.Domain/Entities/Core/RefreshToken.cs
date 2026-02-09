using System;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents a refresh token for JWT authentication
    /// Stores refresh tokens to enable token rotation and revocation
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string Token { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public string? CreatedByIp { get; private set; }

        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public string? RevokedByIp { get; private set; }

        public string? ReplacedByToken { get; private set; }

        public string? RevocationReason { get; private set; }

        // Navigation Property
        public virtual User? User { get; private set; }

        // Computed Properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        // EF Core requires a parameterless constructor
        private RefreshToken()
        {
            Token = string.Empty;
        }

        private RefreshToken(
            Guid userId,
            string token,
            DateTime expiresAt,
            string? createdByIp = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Token = ValidateToken(token);
            ExpiresAt = ValidateExpiresAt(expiresAt);
            CreatedAt = DateTime.UtcNow;
            CreatedByIp = createdByIp;
            IsRevoked = false;
        }

        /// <summary>
        /// Factory method to create a new refresh token
        /// </summary>
        public static RefreshToken Create(
            Guid userId,
            string token,
            DateTime expiresAt,
            string? createdByIp = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            return new RefreshToken(userId, token, expiresAt, createdByIp);
        }

        /// <summary>
        /// Revokes the refresh token
        /// </summary>
        public void Revoke(string? revokedByIp = null, string? reason = null, string? replacedByToken = null)
        {
            if (IsRevoked)
                throw new InvalidOperationException($"Token '{Id}' is already revoked.");

            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            RevocationReason = reason;
            ReplacedByToken = replacedByToken;
        }

        /// <summary>
        /// Checks if the token can be used (not revoked and not expired)
        /// </summary>
        public bool CanBeUsed()
        {
            return IsActive;
        }

        private static string ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty.", nameof(token));

            if (token.Length > 500)
                throw new ArgumentException("Token cannot exceed 500 characters.", nameof(token));

            return token;
        }

        private static DateTime ValidateExpiresAt(DateTime expiresAt)
        {
            if (expiresAt <= DateTime.UtcNow)
                throw new ArgumentException("Expiration date must be in the future.", nameof(expiresAt));

            return expiresAt;
        }
    }
}
