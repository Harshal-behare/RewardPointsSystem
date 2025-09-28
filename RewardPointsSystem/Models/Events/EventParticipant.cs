using System;

namespace RewardPointsSystem.Models.Events
{
    public class EventParticipant
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int? PointsAwarded { get; set; }
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