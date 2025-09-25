using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Models
{
    public class Role
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string Description { get; set; }
        public HashSet<string> Permissions { get; private set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Role(string name, string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required", nameof(name));

            Name = name;
            Description = description;
            Permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddPermission(string permission)
        {
            if (string.IsNullOrWhiteSpace(permission))
                throw new ArgumentException("Permission cannot be empty", nameof(permission));

            Permissions.Add(permission);
        }

        public void RemovePermission(string permission)
        {
            Permissions.Remove(permission);
        }

        public bool HasPermission(string permission)
        {
            return !string.IsNullOrWhiteSpace(permission) && Permissions.Contains(permission);
        }
    }

    // Define standard permissions
    public static class Permissions
    {
        public const string ViewUsers = "view_users";
        public const string ManageUsers = "manage_users";
        public const string ViewProducts = "view_products";
        public const string ManageProducts = "manage_products";
        public const string ViewEvents = "view_events";
        public const string ManageEvents = "manage_events";
        public const string ViewRedemptions = "view_redemptions";
        public const string ApproveRedemptions = "approve_redemptions";
        public const string ManageInventory = "manage_inventory";
        public const string ViewReports = "view_reports";
        public const string ManageRoles = "manage_roles";
        public const string GrantPoints = "grant_points";
    }
}