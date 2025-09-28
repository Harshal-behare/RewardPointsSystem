using System;

namespace RewardPointsSystem.Models.Core
{
    public class UserRole
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public Guid AssignedBy { get; set; }

        public UserRole()
        {
            Id = Guid.NewGuid();
            AssignedAt = DateTime.UtcNow;
        }
    }
}