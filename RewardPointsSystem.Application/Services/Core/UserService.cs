using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Application.Services.Core
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
            var normalizedEmail = email?.Trim().ToLowerInvariant();
            var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);
            if (existingUser != null)
                throw new DuplicateUserEmailException(email!);

            var user = User.Create(email, firstName, lastName);

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

            var email = !string.IsNullOrWhiteSpace(updates.Email) ? updates.Email : user.Email;
            var firstName = !string.IsNullOrWhiteSpace(updates.FirstName) ? updates.FirstName : user.FirstName;
            var lastName = !string.IsNullOrWhiteSpace(updates.LastName) ? updates.LastName : user.LastName;

            if (email != user.Email)
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                var existingUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);
                if (existingUser != null && existingUser.Id != id)
                    throw new DuplicateUserEmailException(email);
            }

            user.UpdateInfo(email, firstName, lastName, id);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            
            return user;
        }

        public async Task DeactivateUserAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new UserNotFoundException(id);

            user.Deactivate(id);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}