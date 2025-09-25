using System;
using System.Collections.Generic;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class RoleService : IRoleService
    {
        private readonly List<Role> _roles = new();
        private readonly object _lockObject = new();

        public RoleService()
        {
            // Initialize default roles
            InitializeDefaultRoles();
        }

        private void InitializeDefaultRoles()
        {
            // Create default Employee role
            var employeeRole = CreateRole("Employee", "Standard employee role");
            employeeRole.AddPermission(Permissions.ViewProducts);
            employeeRole.AddPermission(Permissions.ViewEvents);
            employeeRole.AddPermission(Permissions.ViewRedemptions);

            // Create default Admin role
            var adminRole = CreateRole("Admin", "Administrator role with full permissions");
            adminRole.AddPermission(Permissions.ViewUsers);
            adminRole.AddPermission(Permissions.ManageUsers);
            adminRole.AddPermission(Permissions.ViewProducts);
            adminRole.AddPermission(Permissions.ManageProducts);
            adminRole.AddPermission(Permissions.ViewEvents);
            adminRole.AddPermission(Permissions.ManageEvents);
            adminRole.AddPermission(Permissions.ViewRedemptions);
            adminRole.AddPermission(Permissions.ApproveRedemptions);
            adminRole.AddPermission(Permissions.ManageInventory);
            adminRole.AddPermission(Permissions.ViewReports);
            adminRole.AddPermission(Permissions.ManageRoles);
            adminRole.AddPermission(Permissions.GrantPoints);

            // Create Manager role
            var managerRole = CreateRole("Manager", "Manager role with elevated permissions");
            managerRole.AddPermission(Permissions.ViewUsers);
            managerRole.AddPermission(Permissions.ViewProducts);
            managerRole.AddPermission(Permissions.ViewEvents);
            managerRole.AddPermission(Permissions.ManageEvents);
            managerRole.AddPermission(Permissions.ViewRedemptions);
            managerRole.AddPermission(Permissions.ApproveRedemptions);
            managerRole.AddPermission(Permissions.ViewReports);
            managerRole.AddPermission(Permissions.GrantPoints);
        }

        public Role CreateRole(string name, string description)
        {
            lock (_lockObject)
            {
                if (_roles.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"Role with name '{name}' already exists");

                var role = new Role(name, description);
                _roles.Add(role);
                return role;
            }
        }

        public Role GetRoleById(Guid roleId)
        {
            lock (_lockObject)
            {
                return _roles.FirstOrDefault(r => r.Id == roleId);
            }
        }

        public Role GetRoleByName(string name)
        {
            lock (_lockObject)
            {
                return _roles.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
        }

        public IEnumerable<Role> GetAllRoles()
        {
            lock (_lockObject)
            {
                return _roles.Where(r => r.IsActive).ToList();
            }
        }

        public void UpdateRole(Role role)
        {
            lock (_lockObject)
            {
                var existingRole = GetRoleById(role.Id);
                if (existingRole == null)
                    throw new InvalidOperationException($"Role with ID {role.Id} not found");

                // In a real system, you'd update the role in the database
                // For in-memory, the role object is already updated since it's a reference
            }
        }

        public void DeleteRole(Guid roleId)
        {
            lock (_lockObject)
            {
                var role = GetRoleById(roleId);
                if (role == null)
                    throw new InvalidOperationException($"Role with ID {roleId} not found");

                // Soft delete - just mark as inactive
                role.IsActive = false;
            }
        }

        public void AssignPermissionToRole(Guid roleId, string permission)
        {
            lock (_lockObject)
            {
                var role = GetRoleById(roleId);
                if (role == null)
                    throw new InvalidOperationException($"Role with ID {roleId} not found");

                role.AddPermission(permission);
            }
        }

        public void RemovePermissionFromRole(Guid roleId, string permission)
        {
            lock (_lockObject)
            {
                var role = GetRoleById(roleId);
                if (role == null)
                    throw new InvalidOperationException($"Role with ID {roleId} not found");

                role.RemovePermission(permission);
            }
        }

        public bool UserHasPermission(User user, string permission)
        {
            lock (_lockObject)
            {
                if (user == null || string.IsNullOrWhiteSpace(permission))
                    return false;

                return user.RoleIds
                    .Select(roleId => GetRoleById(roleId))
                    .Where(role => role != null && role.IsActive)
                    .Any(role => role.HasPermission(permission));
            }
        }

        public IEnumerable<string> GetUserPermissions(User user)
        {
            lock (_lockObject)
            {
                if (user == null)
                    return Enumerable.Empty<string>();

                return user.RoleIds
                    .Select(roleId => GetRoleById(roleId))
                    .Where(role => role != null && role.IsActive)
                    .SelectMany(role => role.Permissions)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }
    }
}