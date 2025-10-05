using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Interfaces
{
    /// <summary>
    /// Interface: IUserService
    /// Responsibility: Manage user accounts only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IUserService
    {
        Task<User> CreateUserAsync(string email, string firstName, string lastName);
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User> UpdateUserAsync(Guid id, UserUpdateDto updates);
        Task DeactivateUserAsync(Guid id);
    }

    /// <summary>
    /// DTO for updating user information - Architecture Compliant
    /// </summary>
    public class UserUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
