using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Domain.Entities.Events
{
    /// <summary>
    /// Represents a user's participation in an event
    /// </summary>
    public class EventParticipant
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Event ID is required")]
        public Guid EventId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Points awarded cannot be negative")]
        public int? PointsAwarded { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Position must be a positive number")]
        public int? Position { get; set; }

        public DateTime RegisteredAt { get; set; }
        public DateTime? AwardedAt { get; set; }
        public Guid? AwardedBy { get; set; }

        public EventParticipant()
        {
            Id = Guid.NewGuid();
            RegisteredAt = DateTime.UtcNow;
        }
    }
}
