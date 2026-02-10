using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IRoleService
    /// Responsibility: Manage system roles only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(string name, string description);
        Task<Role> GetRoleByIdAsync(Guid id);
        Task<Role> GetRoleByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> UpdateRoleAsync(Guid id, string name, string description);
        Task DeleteRoleAsync(Guid id);
    }
}