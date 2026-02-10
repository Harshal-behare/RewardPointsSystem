using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service interface for JWT token generation, validation, and management
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token for the given user with their roles
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="roles">List of roles assigned to the user</param>
        /// <returns>JWT access token string</returns>
        string GenerateAccessToken(User user, IEnumerable<Role> roles);

        /// <summary>
        /// Generates a secure random refresh token
        /// </summary>
        /// <returns>Base64-encoded refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates a JWT token and extracts claims
        /// </summary>
        /// <param name="token">JWT access token to validate</param>
        /// <returns>ClaimsPrincipal if valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Extracts the user ID from a JWT token without validation
        /// </summary>
        /// <param name="token">JWT access token</param>
        /// <returns>User ID if token contains NameIdentifier claim, null otherwise</returns>
        Guid? GetUserIdFromToken(string token);

        /// <summary>
        /// Stores a refresh token for a user in the database
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="refreshToken">Refresh token string</param>
        /// <param name="expiresAt">Expiration date and time</param>
        /// <param name="createdByIp">IP address that created the token</param>
        /// <returns>Task</returns>
        Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, string? createdByIp = null);

        /// <summary>
        /// Validates a refresh token and returns the associated user ID
        /// </summary>
        /// <param name="refreshToken">Refresh token string to validate</param>
        /// <returns>User ID if valid, null if invalid or expired</returns>
        Task<Guid?> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token to revoke</param>
        /// <param name="revokedByIp">IP address that revoked the token</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="replacedByToken">New token that replaced this one</param>
        /// <returns>True if revoked successfully, false otherwise</returns>
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp = null, string? reason = null, string? replacedByToken = null);

        /// <summary>
        /// Revokes all refresh tokens for a specific user (e.g., on logout from all devices)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="revokedByIp">IP address that revoked the tokens</param>
        /// <param name="reason">Reason for revocation</param>
        /// <returns>Number of tokens revoked</returns>
        Task<int> RevokeAllUserRefreshTokensAsync(Guid userId, string? revokedByIp = null, string? reason = null);
    }
}
