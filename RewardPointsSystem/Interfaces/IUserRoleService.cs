using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Core;

namespace RewardPointsSystem.Interfaces
{
    public interface IUserRoleService
    {
        Task AssignRoleAsync(Guid userId, Guid roleId, Guid assignedBy);
        Task RemoveRoleAsync(Guid userId, Guid roleId);
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
        Task<bool> IsUserInRoleAsync(Guid userId, string roleName);
        Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName);
    }
}