using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(string email, string employeeId, string firstName, string lastName);
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByEmployeeIdAsync(string employeeId);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task UpdateUserAsync(Guid id, UserUpdateDto updates);
        Task DeactivateUserAsync(Guid id);
    }

    public class UserUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}