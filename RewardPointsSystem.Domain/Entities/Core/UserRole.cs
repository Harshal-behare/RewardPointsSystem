using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents the relationship between a user and their assigned role
    /// Uses composite key (UserId + RoleId) for identification
    /// </summary>
    public class UserRole
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        public Guid RoleId { get; set; }

        public DateTime AssignedAt { get; set; }

        [Required(ErrorMessage = "Assigned by user ID is required")]
        public Guid AssignedBy { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }

        public UserRole()
        {
            AssignedAt = DateTime.UtcNow;
        }
    }
}
