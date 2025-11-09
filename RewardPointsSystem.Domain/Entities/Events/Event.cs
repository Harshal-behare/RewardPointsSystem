using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;

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
    /// Represents an event in the reward points system with state machine logic
    /// </summary>
    public class Event
    {
        private readonly List<EventParticipant> _participants;

        public Guid Id { get; private set; }

        [Required(ErrorMessage = "Event name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Event name must be between 3 and 200 characters")]
        public string Name { get; private set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; private set; }

        [Required(ErrorMessage = "Event date is required")]
        public DateTime EventDate { get; private set; }

        public EventStatus Status { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total points pool cannot be negative")]
        public int TotalPointsPool { get; private set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max participants must be at least 1")]
        public int? MaxParticipants { get; private set; }

        public DateTime? RegistrationStartDate { get; private set; }
        public DateTime? RegistrationEndDate { get; private set; }

        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string? Location { get; private set; }

        [StringLength(1000, ErrorMessage = "Virtual link cannot exceed 1000 characters")]
        public string? VirtualLink { get; private set; }

        [StringLength(1000, ErrorMessage = "Banner image URL cannot exceed 1000 characters")]
        public string? BannerImageUrl { get; private set; }

        [Required(ErrorMessage = "Created by user ID is required")]
        public Guid CreatedBy { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        // Navigation Properties - Encapsulated collection
        public virtual User? Creator { get; private set; }
        public virtual IReadOnlyCollection<EventParticipant> Participants => _participants.AsReadOnly();

        // EF Core requires a parameterless constructor
        private Event()
        {
            _participants = new List<EventParticipant>();
            Name = string.Empty;
        }

        private Event(
            string name,
            DateTime eventDate,
            int totalPointsPool,
            Guid createdBy,
            string? description = null,
            int? maxParticipants = null,
            DateTime? registrationStartDate = null,
            DateTime? registrationEndDate = null,
            string? location = null,
            string? virtualLink = null,
            string? bannerImageUrl = null) : this()
        {
            Id = Guid.NewGuid();
            Name = ValidateName(name);
            Description = description;
            EventDate = ValidateEventDate(eventDate);
            TotalPointsPool = ValidatePointsPool(totalPointsPool);
            MaxParticipants = maxParticipants;
            RegistrationStartDate = registrationStartDate;
            RegistrationEndDate = registrationEndDate;
            Location = location;
            VirtualLink = virtualLink;
            BannerImageUrl = bannerImageUrl;
            CreatedBy = createdBy;
            Status = EventStatus.Draft;
            CreatedAt = DateTime.UtcNow;

            ValidateRegistrationDates();
        }

        /// <summary>
        /// Factory method to create a new event
        /// </summary>
        public static Event Create(
            string name,
            DateTime eventDate,
            int totalPointsPool,
            Guid createdBy,
            string? description = null,
            int? maxParticipants = null,
            DateTime? registrationStartDate = null,
            DateTime? registrationEndDate = null,
            string? location = null,
            string? virtualLink = null,
            string? bannerImageUrl = null)
        {
            return new Event(
                name, eventDate, totalPointsPool, createdBy, 
                description, maxParticipants, registrationStartDate, 
                registrationEndDate, location, virtualLink, bannerImageUrl);
        }

        /// <summary>
        /// Updates event details (only in Draft status)
        /// </summary>
        public void UpdateDetails(
            string name,
            DateTime eventDate,
            int totalPointsPool,
            string? description = null,
            int? maxParticipants = null,
            string? location = null,
            string? virtualLink = null,
            string? bannerImageUrl = null)
        {
            if (Status != EventStatus.Draft)
                throw new InvalidEventStateException(Id, "Event details can only be updated in Draft status.");

            Name = ValidateName(name);
            EventDate = ValidateEventDate(eventDate);
            TotalPointsPool = ValidatePointsPool(totalPointsPool);
            Description = description;
            MaxParticipants = maxParticipants;
            Location = location;
            VirtualLink = virtualLink;
            BannerImageUrl = bannerImageUrl;
        }

        /// <summary>
        /// Publishes the event (Draft → Published)
        /// </summary>
        public void Publish()
        {
            if (Status != EventStatus.Draft)
                throw new InvalidEventStateException(Id, $"Cannot publish event from {Status} status.");

            Status = EventStatus.Published;
        }

        /// <summary>
        /// Opens registration (Published/RegistrationClosed → RegistrationOpen)
        /// </summary>
        public void OpenRegistration()
        {
            if (Status != EventStatus.Published && Status != EventStatus.RegistrationClosed)
                throw new InvalidEventStateException(Id, $"Cannot open registration from {Status} status.");

            if (EventDate <= DateTime.UtcNow)
                throw new InvalidEventStateException(Id, "Cannot open registration for past events.");

            Status = EventStatus.RegistrationOpen;
        }

        /// <summary>
        /// Closes registration (RegistrationOpen → RegistrationClosed)
        /// </summary>
        public void CloseRegistration()
        {
            if (Status != EventStatus.RegistrationOpen)
                throw new InvalidEventStateException(Id, $"Cannot close registration from {Status} status.");

            Status = EventStatus.RegistrationClosed;
        }

        /// <summary>
        /// Starts the event (RegistrationClosed/Published → InProgress)
        /// </summary>
        public void Start()
        {
            if (Status != EventStatus.RegistrationClosed && Status != EventStatus.Published)
                throw new InvalidEventStateException(Id, $"Cannot start event from {Status} status.");

            Status = EventStatus.InProgress;
        }

        /// <summary>
        /// Completes the event (InProgress → Completed)
        /// </summary>
        public void Complete()
        {
            if (Status != EventStatus.InProgress)
                throw new InvalidEventStateException(Id, $"Cannot complete event from {Status} status.");

            Status = EventStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Cancels the event (any status except Completed → Cancelled)
        /// </summary>
        public void Cancel()
        {
            if (Status == EventStatus.Completed)
                throw new InvalidEventStateException(Id, "Cannot cancel a completed event.");

            if (Status == EventStatus.Cancelled)
                throw new InvalidEventStateException(Id, "Event is already cancelled.");

            Status = EventStatus.Cancelled;
        }

        /// <summary>
        /// Checks if registration is currently open
        /// </summary>
        public bool IsRegistrationOpen()
        {
            return Status == EventStatus.RegistrationOpen &&
                   EventDate > DateTime.UtcNow &&
                   (!MaxParticipants.HasValue || _participants.Count < MaxParticipants.Value);
        }

        /// <summary>
        /// Checks if event has reached max participants
        /// </summary>
        public bool HasReachedMaxParticipants()
        {
            return MaxParticipants.HasValue && _participants.Count >= MaxParticipants.Value;
        }

        /// <summary>
        /// Gets available points in the pool
        /// </summary>
        public int GetAvailablePointsPool()
        {
            var awardedPoints = _participants
                .Where(p => p.PointsAwarded.HasValue)
                .Sum(p => p.PointsAwarded.Value);

            return TotalPointsPool - awardedPoints;
        }

        /// <summary>
        /// Internal method to add a participant (used by EventParticipant)
        /// </summary>
        internal void AddParticipant(EventParticipant participant)
        {
            if (participant == null)
                throw new ArgumentNullException(nameof(participant));

            if (_participants.Any(p => p.UserId == participant.UserId))
                throw new UserAlreadyRegisteredForEventException(participant.UserId, Id);

            _participants.Add(participant);
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidEventDataException("Event name is required.");

            if (name.Length < 3 || name.Length > 200)
                throw new InvalidEventDataException("Event name must be between 3 and 200 characters.");

            return name.Trim();
        }

        private static DateTime ValidateEventDate(DateTime eventDate)
        {
            if (eventDate <= DateTime.UtcNow)
                throw new InvalidEventDataException("Event date must be in the future.");

            return eventDate;
        }

        private static int ValidatePointsPool(int pointsPool)
        {
            if (pointsPool < 0)
                throw new InvalidEventDataException("Total points pool cannot be negative.");

            return pointsPool;
        }

        private void ValidateRegistrationDates()
        {
            if (RegistrationStartDate.HasValue && RegistrationEndDate.HasValue)
            {
                if (RegistrationEndDate.Value <= RegistrationStartDate.Value)
                    throw new InvalidEventDataException("Registration end date must be after start date.");

                if (RegistrationEndDate.Value >= EventDate)
                    throw new InvalidEventDataException("Registration must end before event date.");
            }
        }
    }
}
