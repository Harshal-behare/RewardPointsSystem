using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Domain.Entities.Events
{
    public enum EventStatus
    {
        Draft,
        Published,
        RegistrationOpen,
        RegistrationClosed,
        InProgress,
        Completed,
        Cancelled,
        // Legacy statuses for backward compatibility
        Upcoming = Published,
        Active = InProgress
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

        // Exception should be displayed
        [Range(0, int.MaxValue, ErrorMessage = "Total points pool cannot be negative")]
        public int TotalPointsPool { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max participants must be at least 1")]
        public int? MaxParticipants { get; set; }

        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }

        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string? Location { get; set; }

        [StringLength(1000, ErrorMessage = "Virtual link cannot exceed 1000 characters")]
        public string? VirtualLink { get; set; }

        [StringLength(1000, ErrorMessage = "Banner image URL cannot exceed 1000 characters")]
        public string? BannerImageUrl { get; set; }

        [Required(ErrorMessage = "Created by user ID is required")]
        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        public virtual User Creator { get; set; }
        public virtual ICollection<EventParticipant> Participants { get; set; }

        public Event()
        {
            Id = Guid.NewGuid();
            Status = EventStatus.Draft;
            CreatedAt = DateTime.UtcNow;
            Participants = new HashSet<EventParticipant>();
        }
    }
}
