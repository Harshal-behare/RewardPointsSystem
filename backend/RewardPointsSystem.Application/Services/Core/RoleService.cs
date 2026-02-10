using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Services.Core
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

            var existingRole = await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == name);
            if (existingRole != null)
                throw new InvalidOperationException($"Role with name {name} already exists");

            var role = Role.Create(name, description);

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
            return role;
        }

        public async Task<Role> GetRoleByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required", nameof(name));

            return await _unitOfWork.Roles.SingleOrDefaultAsync(r => r.Name == name);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _unitOfWork.Roles.GetAllAsync();
        }

        public async Task<Role> GetRoleByIdAsync(Guid id)
        {
            return await _unitOfWork.Roles.GetByIdAsync(id);
        }

        public async Task<Role> UpdateRoleAsync(Guid id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException($"Role with ID {id} not found");

            role.UpdateInfo(name, description);
            await _unitOfWork.Roles.UpdateAsync(role);
            await _unitOfWork.SaveChangesAsync();
            return role;
        }

        public async Task DeleteRoleAsync(Guid id)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role == null)
                throw new InvalidOperationException($"Role with ID {id} not found");

            await _unitOfWork.Roles.DeleteAsync(role);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
