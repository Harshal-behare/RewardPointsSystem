using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents a role in the system (e.g., Admin, Employee)
    /// </summary>
    public class Role
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Role name must be between 2 and 50 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; }

        public Role()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UserRoles = new HashSet<UserRole>();
        }
    }
}
