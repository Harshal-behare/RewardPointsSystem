using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Interfaces
{
    /// <summary>
    /// Interface: IRoleService
    /// Responsibility: Manage system roles only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(string name, string description);
        Task<Role> GetRoleByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task UpdateRoleAsync(Guid id, string description);
    }
}