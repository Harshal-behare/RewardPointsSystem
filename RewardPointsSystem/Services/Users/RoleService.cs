using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Role> CreateRoleAsync(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Role description is required", nameof(description));

            // Check for duplicate role name
            var existingRole = await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == name);
            if (existingRole != null)
                throw new InvalidOperationException($"Role with name {name} already exists");

            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return role;
        }

        public async Task<Role> UpdateRoleAsync(Guid id, string name, string description)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException($"Role with ID {id} not found");

            // Check for name uniqueness if name is being updated
            if (!string.IsNullOrWhiteSpace(name) && name != role.Name)
            {
                var existingRole = await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == name);
                if (existingRole != null)
                    throw new InvalidOperationException($"Role with name {name} already exists");
                
                role.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(description))
                role.Description = description;

            role.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Roles.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return role;
        }

        public async Task<Role> GetRoleAsync(Guid id)
        {
            return await _unitOfWork.Roles.GetByIdAsync(id);
        }

        public async Task<Role> GetRoleByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required", nameof(name));

            return await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetActiveRolesAsync()
        {
            return await _unitOfWork.Roles.FindAsync(r => r.IsActive);
        }

        public async Task DeactivateRoleAsync(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException($"Role with ID {id} not found");

            // Check if role is still assigned to users
            var assignedUsers = await _unitOfWork.UserRoles.FindAsync(ur => ur.RoleId == id);
            if (assignedUsers.Any())
                throw new InvalidOperationException($"Cannot deactivate role {role.Name} as it is still assigned to users");

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Roles.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}