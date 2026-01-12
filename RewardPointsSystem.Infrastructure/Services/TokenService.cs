using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RewardPointsSystem.Application.Configuration;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Infrastructure.Data;

namespace RewardPointsSystem.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for JWT token generation, validation, and management
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly RewardPointsDbContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenService(JwtSettings jwtSettings, RewardPointsDbContext context)
        {
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero // No clock skew - token expires exactly when specified
            };
        }

        /// <summary>
        /// Generates a JWT access token for the given user with their roles
        /// </summary>
        public string GenerateAccessToken(User user, IEnumerable<Role> roles)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add roles as claims
            if (roles != null)
            {
                foreach (var role in roles.Where(r => r.IsActive))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a secure random refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64]; // 512 bits
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// Validates a JWT token and extracts claims
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                // Verify that the token uses the expected security algorithm
                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                // Token validation failed
                return null;
            }
        }

        /// <summary>
        /// Extracts the user ID from a JWT token without full validation
        /// </summary>
        public Guid? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Stores a refresh token for a user in the database
        /// </summary>
        public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAt, string? createdByIp = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token cannot be empty.", nameof(refreshToken));

            var token = RefreshToken.Create(userId, refreshToken, expiresAt, createdByIp);
            
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates a refresh token and returns the associated user ID
        /// </summary>
        public async Task<Guid?> ValidateRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
                return null;

            // Check if token is still active (not revoked and not expired)
            if (!token.CanBeUsed())
                return null;

            return token.UserId;
        }

        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string? revokedByIp = null, string? reason = null, string? replacedByToken = null)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null || token.IsRevoked)
                return false;

            token.Revoke(revokedByIp, reason, replacedByToken);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Revokes all refresh tokens for a specific user
        /// </summary>
        public async Task<int> RevokeAllUserRefreshTokensAsync(Guid userId, string? revokedByIp = null, string? reason = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            var now = DateTime.UtcNow;
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > now)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoke(revokedByIp, reason ?? "Logout from all devices");
            }

            if (tokens.Any())
            {
                await _context.SaveChangesAsync();
            }

            return tokens.Count;
        }
    }
}
