using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<User> CreateUserAsync(string email, string employeeId, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Employee ID is required", nameof(employeeId));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required", nameof(lastName));

            var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {email} already exists");

            var existingEmployee = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (existingEmployee != null)
                throw new InvalidOperationException($"User with employee ID {employeeId} already exists");

            var user = new User
            {
                Email = email,
                EmployeeId = employeeId,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required", nameof(email));

            return await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByEmployeeIdAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException("Employee ID is required", nameof(employeeId));

            return await _unitOfWork.Users.SingleOrDefaultAsync(u => u.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _unitOfWork.Users.FindAsync(u => u.IsActive);
        }

        public async Task<User> UpdateUserAsync(Guid id, UserUpdateDto updates)
        {
            if (updates == null)
                throw new ArgumentNullException(nameof(updates));

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(updates.Email) && updates.Email != user.Email)
            {
                var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == updates.Email);
                if (existingUser != null)
                    throw new InvalidOperationException($"User with email {updates.Email} already exists");
                user.Email = updates.Email;
            }

            if (!string.IsNullOrWhiteSpace(updates.FirstName))
                user.FirstName = updates.FirstName;

            if (!string.IsNullOrWhiteSpace(updates.LastName))
                user.LastName = updates.LastName;

            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        public async Task DeactivateUserAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}