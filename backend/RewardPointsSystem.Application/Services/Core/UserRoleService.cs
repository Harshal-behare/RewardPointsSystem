using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Services.Core
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task AssignRoleAsync(Guid userId, Guid roleId, Guid assignedBy)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role == null || !role.IsActive)
                throw new InvalidOperationException($"Role with ID {roleId} not found or inactive");

            var existingAssignment = await _unitOfWork.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (existingAssignment != null)
                throw new InvalidOperationException($"User is already assigned to this role");

            var userRole = UserRole.Assign(userId, roleId, assignedBy);

            await _unitOfWork.UserRoles.AddAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _unitOfWork.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null)
                throw new InvalidOperationException($"User is not assigned to this role");

            await _unitOfWork.UserRoles.DeleteAsync(userRole);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RevokeRoleAsync(Guid userId, Guid roleId)
        {
            // Alias for RemoveRoleAsync
            await RemoveRoleAsync(userId, roleId);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId);
            var roles = new List<Role>();

            foreach (var userRole in userRoles)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(userRole.RoleId);
                if (role != null && role.IsActive)
                    roles.Add(role);
            }

            return roles;
        }

        public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return false;

            var role = await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == roleName && r.IsActive);
            if (role == null)
                return false;

            return await _unitOfWork.UserRoles.ExistsAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
        }

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return new List<User>();

            var role = await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == roleName && r.IsActive);
            if (role == null)
                return new List<User>();

            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.RoleId == role.Id);
            var users = new List<User>();

            foreach (var userRole in userRoles)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userRole.UserId);
                if (user != null && user.IsActive)
                    users.Add(user);
            }

            return users;
        }
    }
}