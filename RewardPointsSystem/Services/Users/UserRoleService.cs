using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<UserRole> AssignRoleAsync(Guid userId, Guid roleId)
        {
            // Validate user exists and is active
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            // Validate role exists and is active
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null || !role.IsActive)
                throw new InvalidOperationException($"Role with ID {roleId} not found or inactive");

            // Check if assignment already exists
            var existingAssignment = await _unitOfWork.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (existingAssignment != null)
                throw new InvalidOperationException($"User {userId} is already assigned to role {roleId}");

            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();

            return userRole;
        }

        public async Task RemoveRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _unitOfWork.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);
            if (userRole == null)
                throw new InvalidOperationException($"User {userId} is not assigned to role {roleId}");

            userRole.IsActive = false;
            userRole.RevokedAt = DateTime.UtcNow;

            await _unitOfWork.UserRoles.UpdateAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(Guid userId)
        {
            return await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId && ur.IsActive);
        }

        public async Task<IEnumerable<UserRole>> GetUsersInRoleAsync(Guid roleId)
        {
            return await _unitOfWork.UserRoles.FindAsync(ur => ur.RoleId == roleId && ur.IsActive);
        }

        public async Task<bool> IsUserInRoleAsync(Guid userId, Guid roleId)
        {
            return await _unitOfWork.UserRoles.ExistsAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.IsActive);
        }

        public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name is required", nameof(roleName));

            var role = await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == roleName && r.IsActive);
            if (role == null)
                return false;

            return await IsUserInRoleAsync(userId, role.Id);
        }
    }
}