using System;
using System.Security.Cryptography;
using System.Text;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Infrastructure.Services
{
    /// <summary>
    /// Password hashing service using PBKDF2 with HMACSHA256
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8; // 128 bits
        private const int HashSize = 256 / 8; // 256 bits
        private const int Iterations = 100000; // OWASP recommended minimum

        /// <summary>
        /// Hashes a password using PBKDF2 with HMACSHA256
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using Rfc2898DeriveBytes (PBKDF2)
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // Combine salt and hash
                byte[] hashBytes = new byte[SaltSize + HashSize];
                Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                // Convert to base64 for storage
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(passwordHash))
                return false;

            try
            {
                // Convert base64 hash back to bytes
                byte[] hashBytes = Convert.FromBase64String(passwordHash);

                // Ensure the hash is the correct length
                if (hashBytes.Length != SaltSize + HashSize)
                    return false;

                // Extract the salt
                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                // Extract the hash
                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

                // Hash the provided password with the extracted salt
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    byte[] computedHash = pbkdf2.GetBytes(HashSize);

                    // Compare the hashes using constant-time comparison
                    return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
