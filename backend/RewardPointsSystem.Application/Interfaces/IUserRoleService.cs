using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IUserRoleService
    /// Responsibility: Manage user-role assignments only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IUserRoleService
    {
        Task AssignRoleAsync(Guid userId, Guid roleId, Guid assignedBy);
        Task RemoveRoleAsync(Guid userId, Guid roleId);
        Task RevokeRoleAsync(Guid userId, Guid roleId);
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
        Task<bool> IsUserInRoleAsync(Guid userId, string roleName);
        Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName);
    }
}