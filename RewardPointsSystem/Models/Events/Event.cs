using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Models.Events
{
    public enum EventStatus
    {
        Upcoming,
        Active,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Represents an event in the reward points system
    /// </summary>
    public class Event
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Event name must be between 3 and 200 characters")]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; set; }

        public EventStatus Status { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total points pool cannot be negative")]
        public int TotalPointsPool { get; set; }

        [Required(ErrorMessage = "Created by user ID is required")]
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
