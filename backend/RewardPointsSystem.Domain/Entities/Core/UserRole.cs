using System;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Represents the relationship between a user and their assigned role
    /// Uses composite key (UserId + RoleId) for identification
    /// </summary>
    public class UserRole
    {
        public Guid UserId { get; private set; }

        public Guid RoleId { get; private set; }

        public DateTime AssignedAt { get; private set; }

        public Guid AssignedBy { get; private set; }

        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual User? User { get; private set; }
        public virtual Role? Role { get; private set; }

        // EF Core requires a parameterless constructor
        private UserRole()
        {
        }

        private UserRole(Guid userId, Guid roleId, Guid assignedBy)
        {
            UserId = userId;
            RoleId = roleId;
            AssignedBy = assignedBy;
            AssignedAt = DateTime.UtcNow;
            IsActive = true;
        }

        /// <summary>
        /// Factory method to assign a role to a user
        /// </summary>
        public static UserRole Assign(Guid userId, Guid roleId, Guid assignedBy)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Role ID cannot be empty.", nameof(roleId));

            if (assignedBy == Guid.Empty)
                throw new ArgumentException("Assigned by user ID cannot be empty.", nameof(assignedBy));

            return new UserRole(userId, roleId, assignedBy);
        }

        /// <summary>
        /// Deactivates the role assignment
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException("User role assignment is already inactive.");

            IsActive = false;
        }

        /// <summary>
        /// Reactivates the role assignment
        /// </summary>
        public void Reactivate()
        {
            if (IsActive)
                throw new InvalidOperationException("User role assignment is already active.");

            IsActive = true;
        }
    }
}
