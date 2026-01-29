using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents a role in the system (e.g., Admin, Employee)
    /// </summary>
    public class Role
    {
        private readonly List<UserRole> _userRoles;

        public Guid Id { get; private set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
        public string Name { get; private set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; private set; }

        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

       
        public virtual IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

       
        private Role()
        {
            _userRoles = new List<UserRole>();
            Name = string.Empty;
        }

        private Role(string name, string? description = null) : this()
        {
            Id = Guid.NewGuid();
            Name = ValidateName(name);
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new role
        /// </summary>
        public static Role Create(string name, string? description = null)
        {
            return new Role(name, description);
        }

        /// <summary>
        /// Updates role information
        /// </summary>
        public void UpdateInfo(string name, string? description = null)
        {
            Name = ValidateName(name);
            Description = description;
        }

        /// <summary>
        /// Activates the role
        /// </summary>
        public void Activate()
        {
            if (IsActive)
                throw new RoleValidationException($"Role '{Id}' is already active.");

            IsActive = true;
        }

        /// <summary>
        /// Deactivates the role
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new RoleValidationException($"Role '{Id}' is already inactive.");

            IsActive = false;
        }

        /// <summary>
        /// Gets active users assigned to this role
        /// </summary>
        public int GetActiveUserCount()
        {
            return _userRoles.Count(ur => ur.IsActive);
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new RoleValidationException("Role name is required.");

            if (name.Length < 2 || name.Length > 50)
                throw new RoleValidationException("Role name must be between 2 and 50 characters.");

            return name.Trim();
        }
    }
}
