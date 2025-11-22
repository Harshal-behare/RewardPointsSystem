namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service interface for password hashing and verification
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a password using a secure hashing algorithm
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Hashed password</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="passwordHash">Stored password hash</param>
        /// <returns>True if password matches, false otherwise</returns>
        bool VerifyPassword(string password, string passwordHash);
    }
}
