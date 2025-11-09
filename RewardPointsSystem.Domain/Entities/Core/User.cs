using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents a user in the reward points system
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual UserPointsAccount UserPointsAccount { get; set; }
        public virtual ICollection<EventParticipant> EventParticipations { get; set; }
        public virtual ICollection<Redemption> Redemptions { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UserRoles = new HashSet<UserRole>();
            EventParticipations = new HashSet<EventParticipant>();
            Redemptions = new HashSet<Redemption>();
        }
    }
}
