using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<User> CreateUserAsync(string email, string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidUserDataException("Email is required");
            if (string.IsNullOrWhiteSpace(firstName))
                throw new InvalidUserDataException("First name is required");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidUserDataException("Last name is required");

            var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
                throw new DuplicateUserEmailException(email);

            var user = new User
            {
                Email = email,
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
                throw new InvalidUserDataException("Email is required");

            return await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == email);
        }


        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _unitOfWork.Users.FindAsync(u => u.IsActive);
        }

        public async Task<User> UpdateUserAsync(Guid id, UserUpdateDto updates)
        {
            if (updates == null)
                throw new InvalidUserDataException("Update data cannot be null");

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new UserNotFoundException(id);

            if (!string.IsNullOrWhiteSpace(updates.Email) && updates.Email != user.Email)
            {
                var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == updates.Email);
                if (existingUser != null)
                    throw new DuplicateUserEmailException(updates.Email);
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
                throw new UserNotFoundException(id);

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}