using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Events
{
    /// <summary>
    /// Event status: Draft (admin only), Upcoming (employees can register), Active (event in progress), Completed (event finished)
    /// </summary>
    public enum EventStatus
    {
        /// <summary>
        /// Event is in draft state - only visible to admin, not to employees
        /// </summary>
        Draft = 0,
        
        /// <summary>
        /// Event is upcoming - visible to employees who can register
        /// </summary>
        Upcoming = 1,
        
        /// <summary>
        /// Event is active - currently in progress
        /// </summary>
        Active = 2,
        
        /// <summary>
        /// Event is completed - no more registrations, points can be awarded
        /// </summary>
        Completed = 3
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

        // Event End Date - when the event ends (EventDate is when it starts)
        public DateTime? EventEndDate { get; private set; }

        // Prize Distribution for ranks (points to award for 1st, 2nd, 3rd place)
        [Range(0, int.MaxValue, ErrorMessage = "First place points cannot be negative")]
        public int? FirstPlacePoints { get; private set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Second place points cannot be negative")]
        public int? SecondPlacePoints { get; private set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Third place points cannot be negative")]
        public int? ThirdPlacePoints { get; private set; }

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
            string? bannerImageUrl = null,
            DateTime? eventEndDate = null,
            int? firstPlacePoints = null,
            int? secondPlacePoints = null,
            int? thirdPlacePoints = null) : this()
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
            EventEndDate = eventEndDate;
            FirstPlacePoints = firstPlacePoints;
            SecondPlacePoints = secondPlacePoints;
            ThirdPlacePoints = thirdPlacePoints;
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
            string? bannerImageUrl = null,
            DateTime? eventEndDate = null,
            int? firstPlacePoints = null,
            int? secondPlacePoints = null,
            int? thirdPlacePoints = null)
        {
            return new Event(
                name, eventDate, totalPointsPool, createdBy, 
                description, maxParticipants, registrationStartDate, 
                registrationEndDate, location, virtualLink, bannerImageUrl,
                eventEndDate, firstPlacePoints, secondPlacePoints, thirdPlacePoints);
        }

        /// <summary>
        /// Updates event details (not allowed for Completed events)
        /// </summary>
        public void UpdateDetails(
            string name,
            DateTime eventDate,
            int totalPointsPool,
            string? description = null,
            int? maxParticipants = null,
            string? location = null,
            string? virtualLink = null,
            string? bannerImageUrl = null,
            DateTime? eventEndDate = null,
            DateTime? registrationStartDate = null,
            DateTime? registrationEndDate = null,
            int? firstPlacePoints = null,
            int? secondPlacePoints = null,
            int? thirdPlacePoints = null)
        {
            if (Status == EventStatus.Completed)
                throw new InvalidEventStateException(Id, "Cannot update completed events.");

            Name = ValidateName(name);
            EventDate = eventDate; // Allow any date for updates
            TotalPointsPool = ValidatePointsPool(totalPointsPool);
            Description = description;
            MaxParticipants = maxParticipants;
            Location = location;
            VirtualLink = virtualLink;
            BannerImageUrl = bannerImageUrl;
            EventEndDate = eventEndDate;
            RegistrationStartDate = registrationStartDate;
            RegistrationEndDate = registrationEndDate;
            FirstPlacePoints = firstPlacePoints;
            SecondPlacePoints = secondPlacePoints;
            ThirdPlacePoints = thirdPlacePoints;
        }

        /// <summary>
        /// Publishes the event (Draft → Upcoming) - makes it visible to employees
        /// </summary>
        public void Publish()
        {
            if (Status != EventStatus.Draft)
                throw new InvalidEventStateException(Id, $"Cannot publish event from {Status} status. Only Draft events can be published.");

            Status = EventStatus.Upcoming;
        }

        /// <summary>
        /// Marks event as upcoming (alias for Publish)
        /// </summary>
        public void MakeUpcoming()
        {
            Publish();
        }

        /// <summary>
        /// Activates the event (Upcoming → Active) - event is currently in progress
        /// </summary>
        public void Activate()
        {
            if (Status != EventStatus.Upcoming)
                throw new InvalidEventStateException(Id, $"Cannot activate event from {Status} status. Only Upcoming events can be activated.");

            Status = EventStatus.Active;
        }

        /// <summary>
        /// Marks event as active (alias for Activate)
        /// </summary>
        public void MakeActive()
        {
            Activate();
        }

        /// <summary>
        /// Completes the event (Upcoming or Active → Completed)
        /// </summary>
        public void Complete()
        {
            if (Status != EventStatus.Upcoming && Status != EventStatus.Active)
                throw new InvalidEventStateException(Id, $"Cannot complete event from {Status} status. Only Upcoming or Active events can be completed.");

            Status = EventStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Reverts event back to Draft (Upcoming → Draft)
        /// </summary>
        public void RevertToDraft()
        {
            if (Status != EventStatus.Upcoming)
                throw new InvalidEventStateException(Id, $"Cannot revert to draft from {Status} status.");

            Status = EventStatus.Draft;
        }

        /// <summary>
        /// Checks if registration is currently open (only for Upcoming or Active events)
        /// </summary>
        public bool IsRegistrationOpen()
        {
            return (Status == EventStatus.Upcoming || Status == EventStatus.Active) &&
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
                .Sum(p => p.PointsAwarded!.Value);

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
