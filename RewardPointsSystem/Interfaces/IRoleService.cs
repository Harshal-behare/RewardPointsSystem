using System;
using System.Collections.Generic;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IRoleService
    {
        Role CreateRole(string name, string description);
        Role GetRoleById(Guid roleId);
        Role GetRoleByName(string name);
        IEnumerable<Role> GetAllRoles();
        void UpdateRole(Role role);
        void DeleteRole(Guid roleId);
        void AssignPermissionToRole(Guid roleId, string permission);
        void RemovePermissionFromRole(Guid roleId, string permission);
        bool UserHasPermission(User user, string permission);
        IEnumerable<string> GetUserPermissions(User user);
    }
}