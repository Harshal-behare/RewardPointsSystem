using System;

namespace RewardPointsSystem.Models.Events
{
    public enum EventStatus
    {
        Upcoming,
        Active,
        Completed,
        Cancelled
    }

    public class Event
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public EventStatus Status { get; set; }
        public int TotalPointsPool { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public Event()
        {
            Id = Guid.NewGuid();
            Status = EventStatus.Upcoming;
            CreatedAt = DateTime.UtcNow;
        }
    }
}