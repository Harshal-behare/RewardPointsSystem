using System;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Domain.Entities.Events
{
    public enum AttendanceStatus
    {
        Registered,
        CheckedIn,
        Attended,
        NoShow,
        Cancelled
    }

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

        [Range(1, int.MaxValue, ErrorMessage = "Event rank must be a positive number")]
        public int? EventRank { get; set; }

        [Required(ErrorMessage = "Attendance status is required")]
        public AttendanceStatus AttendanceStatus { get; set; }

        public DateTime RegisteredAt { get; set; }
        public DateTime? CheckedInAt { get; set; }
        public DateTime? AwardedAt { get; set; }
        public Guid? AwardedBy { get; set; }

        // Navigation Properties
        public virtual Event Event { get; set; }
        public virtual User User { get; set; }

        public EventParticipant()
        {
            Id = Guid.NewGuid();
            RegisteredAt = DateTime.UtcNow;
            AttendanceStatus = AttendanceStatus.Registered;
        }
    }
}
