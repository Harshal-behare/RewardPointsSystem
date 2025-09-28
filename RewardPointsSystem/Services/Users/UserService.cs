using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<User> CreateUserAsync(CreateUserDto userDto)
        {
            if (userDto == null)
                throw new ArgumentNullException(nameof(userDto));

            if (string.IsNullOrWhiteSpace(userDto.Email))
                throw new ArgumentException("Email is required", nameof(userDto));

            if (string.IsNullOrWhiteSpace(userDto.EmployeeId))
                throw new ArgumentException("EmployeeId is required", nameof(userDto));

            // Check for duplicate email
            var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {userDto.Email} already exists");

            // Check for duplicate employee ID
            var existingEmployee = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.EmployeeId == userDto.EmployeeId);
            if (existingEmployee != null)
                throw new InvalidOperationException($"User with employee ID {userDto.EmployeeId} already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                EmployeeId = userDto.EmployeeId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(Guid id, UpdateUserDto updates)
        {
            if (updates == null)
                throw new ArgumentNullException(nameof(updates));

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new InvalidOperationException($"User with ID {id} not found");

            // Check for email uniqueness if email is being updated
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

        public async Task<User> GetUserAsync(Guid id)
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
                throw new ArgumentException("EmployeeId is required", nameof(employeeId));

            return await _unitOfWork.Users.SingleOrDefaultAsync(u => u.EmployeeId == employeeId);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _unitOfWork.Users.FindAsync(u => u.IsActive);
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

        public async Task<bool> ValidateUserAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null && user.IsActive;
        }
    }
}