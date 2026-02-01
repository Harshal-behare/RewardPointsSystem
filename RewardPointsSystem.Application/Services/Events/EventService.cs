using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Application.Services.Events
{
    /// <summary>
    /// Service: EventService
    /// Responsibility: Manage event lifecycle only
    /// Status flow: Draft → Upcoming → Completed
    /// Architecture Compliant - SRP
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Event> CreateEventAsync(
            string name,
            string description,
            DateTime date,
            int pointsPool,
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
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidEventDataException("Event name is required");

            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidEventDataException("Event description is required");

            // Only validate future date for creation
            if (date < DateTime.UtcNow.Date)
                throw new InvalidEventDataException("Cannot create events in the past");

            // Auto-calculate points pool from rank points if all three are provided
            var calculatedPool = (firstPlacePoints ?? 0) + (secondPlacePoints ?? 0) + (thirdPlacePoints ?? 0);
            if (calculatedPool > 0)
            {
                pointsPool = calculatedPool;
            }

            // Validate points pool (either provided or calculated)
            if (pointsPool <= 0)
                throw new InvalidEventDataException("Points pool must be positive. Provide TotalPointsPool or all three rank points (1st, 2nd, 3rd).");

            // Validate rank order: 1st > 2nd > 3rd
            if (firstPlacePoints.HasValue && secondPlacePoints.HasValue && thirdPlacePoints.HasValue)
            {
                if (secondPlacePoints >= firstPlacePoints)
                    throw new InvalidEventDataException("Second place points must be less than first place points.");
                if (thirdPlacePoints >= secondPlacePoints)
                    throw new InvalidEventDataException("Third place points must be less than second place points.");
            }

            // Get or create system user for event creation
            var systemUser = await GetOrCreateSystemUserAsync();

            var eventEntity = Event.Create(
                name.Trim(),
                date,
                pointsPool,
                systemUser.Id,
                description.Trim(),
                maxParticipants,
                registrationStartDate,
                registrationEndDate,
                location,
                virtualLink,
                bannerImageUrl,
                eventEndDate,
                firstPlacePoints,
                secondPlacePoints,
                thirdPlacePoints);

            await _unitOfWork.Events.AddAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            return eventEntity;
        }

        private async Task<User> GetOrCreateSystemUserAsync()
        {
            // Try to find system user
            var systemUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == "system@rewardpoints.com");
            
            if (systemUser == null)
            {
                // Create system user
                systemUser = User.Create(
                    "system@agdata.com",
                    "System",
                    "Administrator");
                
                await _unitOfWork.Users.AddAsync(systemUser);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return systemUser;
        }

        public async Task<Event> UpdateEventAsync(Guid id, UpdateEventDto updates)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status == EventStatus.Completed && updates.Status?.ToLower() != "completed")
                throw new InvalidEventStateException(id, "Cannot modify completed events");

            var name = !string.IsNullOrWhiteSpace(updates.Name) ? updates.Name.Trim() : eventEntity.Name;
            var description = !string.IsNullOrWhiteSpace(updates.Description) ? updates.Description.Trim() : eventEntity.Description;
            var eventDate = updates.EventDate ?? eventEntity.EventDate;
            var maxParticipants = updates.MaxParticipants ?? eventEntity.MaxParticipants;
            var location = updates.Location ?? eventEntity.Location;
            var virtualLink = updates.VirtualLink ?? eventEntity.VirtualLink;
            var bannerImageUrl = updates.BannerImageUrl ?? eventEntity.BannerImageUrl;
            var eventEndDate = updates.EventEndDate ?? eventEntity.EventEndDate;
            var registrationStartDate = updates.RegistrationStartDate ?? eventEntity.RegistrationStartDate;
            var registrationEndDate = updates.RegistrationEndDate ?? eventEntity.RegistrationEndDate;
            var firstPlacePoints = updates.FirstPlacePoints ?? eventEntity.FirstPlacePoints;
            var secondPlacePoints = updates.SecondPlacePoints ?? eventEntity.SecondPlacePoints;
            var thirdPlacePoints = updates.ThirdPlacePoints ?? eventEntity.ThirdPlacePoints;

            // Auto-calculate points pool from rank points if all three are provided
            var calculatedPool = (firstPlacePoints ?? 0) + (secondPlacePoints ?? 0) + (thirdPlacePoints ?? 0);
            var pointsPool = calculatedPool > 0 ? calculatedPool : (updates.TotalPointsPool ?? eventEntity.TotalPointsPool);

            // Validate rank order: 1st > 2nd > 3rd
            if (firstPlacePoints.HasValue && secondPlacePoints.HasValue && thirdPlacePoints.HasValue)
            {
                if (secondPlacePoints >= firstPlacePoints)
                    throw new InvalidEventDataException("Second place points must be less than first place points.");
                if (thirdPlacePoints >= secondPlacePoints)
                    throw new InvalidEventDataException("Third place points must be less than second place points.");
            }

            eventEntity.UpdateDetails(
                name, 
                eventDate, 
                pointsPool, 
                description, 
                maxParticipants, 
                location, 
                virtualLink, 
                bannerImageUrl,
                eventEndDate,
                registrationStartDate,
                registrationEndDate,
                firstPlacePoints,
                secondPlacePoints,
                thirdPlacePoints);

            // Handle status change if provided - enforce ONE-WAY FLOW ONLY
            if (!string.IsNullOrWhiteSpace(updates.Status))
            {
                var targetStatus = updates.Status.ToLower();
                var currentStatus = eventEntity.Status;

                // Validate one-way flow: Draft → Upcoming → Active → Completed
                // NO backward transitions allowed
                ValidateOneWayStatusTransition(currentStatus, targetStatus, id);

                // Apply status transitions based on target status
                switch (targetStatus)
                {
                    case "upcoming":
                    case "published":
                        if (currentStatus == EventStatus.Draft)
                            eventEntity.Publish();
                        break;
                    case "active":
                        if (currentStatus == EventStatus.Upcoming)
                            eventEntity.Activate();
                        else if (currentStatus == EventStatus.Draft)
                        {
                            eventEntity.Publish();
                            eventEntity.Activate();
                        }
                        break;
                    case "completed":
                        if (currentStatus == EventStatus.Active)
                            eventEntity.Complete();
                        else if (currentStatus == EventStatus.Upcoming)
                        {
                            eventEntity.Activate();
                            eventEntity.Complete();
                        }
                        else if (currentStatus == EventStatus.Draft)
                        {
                            eventEntity.Publish();
                            eventEntity.Activate();
                            eventEntity.Complete();
                        }
                        break;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return eventEntity;
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            // Include participants so we can count them
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => e.Status == EventStatus.Upcoming,
                e => e.Participants);

            // Auto-update statuses before returning
            foreach (var evt in events)
            {
                await UpdateEventStatusBasedOnDatesAsync(evt);
            }

            // Return only still-upcoming events after status update
            return events.Where(e => e.Status == EventStatus.Upcoming);
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            // Include participants so we can count them
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => true,  // All events
                e => e.Participants);

            // Auto-update statuses before returning
            foreach (var evt in events)
            {
                await UpdateEventStatusBasedOnDatesAsync(evt);
            }

            return events;
        }

        /// <summary>
        /// Get visible events for employees (Upcoming, Active, Completed - excludes Draft)
        /// </summary>
        public async Task<IEnumerable<Event>> GetVisibleEventsAsync()
        {
            // Include participants so we can count them
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => true,  // Get all first to auto-update statuses
                e => e.Participants);

            // Auto-update statuses before filtering
            foreach (var evt in events)
            {
                await UpdateEventStatusBasedOnDatesAsync(evt);
            }

            // Return events visible to employees: Upcoming, Active, Completed (not Draft)
            return events.Where(e => e.Status != EventStatus.Draft);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            // Include participants for single event
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => e.Id == id,
                e => e.Participants);
            var eventEntity = events.FirstOrDefault();

            if (eventEntity != null)
            {
                await UpdateEventStatusBasedOnDatesAsync(eventEntity);
            }

            return eventEntity;
        }

        /// <summary>
        /// Auto-update event status based on dates (per System.txt requirements)
        /// Draft → Upcoming: On RegistrationStartDate
        /// Upcoming → Active: On EventDate
        /// Active → Completed: After EventEndDate (or EventDate if no end date)
        /// </summary>
        private async Task UpdateEventStatusBasedOnDatesAsync(Event eventEntity)
        {
            var now = DateTime.UtcNow;
            var originalStatus = eventEntity.Status;

            // Draft → Upcoming: When registration start date is reached
            if (eventEntity.Status == EventStatus.Draft &&
                eventEntity.RegistrationStartDate.HasValue &&
                now >= eventEntity.RegistrationStartDate.Value)
            {
                eventEntity.Publish(); // Changes to Upcoming
            }

            // Upcoming → Active: When event date is reached
            if (eventEntity.Status == EventStatus.Upcoming &&
                now >= eventEntity.EventDate)
            {
                eventEntity.Activate(); // Changes to Active
            }

            // Active → Completed: After event end date (or event date if no end date)
            if (eventEntity.Status == EventStatus.Active)
            {
                var completionDate = eventEntity.EventEndDate ?? eventEntity.EventDate;
                if (now > completionDate)
                {
                    eventEntity.Complete(); // Changes to Completed
                }
            }

            // Save if status changed
            if (eventEntity.Status != originalStatus)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Validate that status transition follows one-way flow: Draft → Upcoming → Active → Completed
        /// </summary>
        private void ValidateOneWayStatusTransition(EventStatus currentStatus, string targetStatusString, Guid eventId)
        {
            // Parse target status
            if (!Enum.TryParse<EventStatus>(targetStatusString, true, out var targetStatus))
            {
                // Try mapping common variations
                targetStatus = targetStatusString.ToLower() switch
                {
                    "published" => EventStatus.Upcoming,
                    _ => throw new InvalidEventStateException(eventId, $"Invalid status value: {targetStatusString}")
                };
            }

            // Define status order for one-way flow
            var statusOrder = new Dictionary<EventStatus, int>
            {
                { EventStatus.Draft, 0 },
                { EventStatus.Upcoming, 1 },
                { EventStatus.Active, 2 },
                { EventStatus.Completed, 3 }
            };

            var currentIndex = statusOrder.GetValueOrDefault(currentStatus, -1);
            var targetIndex = statusOrder.GetValueOrDefault(targetStatus, -1);

            // Same status is allowed (no change)
            if (currentIndex == targetIndex)
                return;

            // Backward transitions are NOT allowed
            if (targetIndex < currentIndex)
            {
                throw new InvalidOperationException(
                    $"Invalid status transition. Events can only move forward: Draft → Upcoming → Active → Completed. " +
                    $"Cannot change from '{currentStatus}' to '{targetStatus}'.");
            }
        }

        /// <summary>
        /// Publish event: Draft → Upcoming (makes event visible to employees)
        /// </summary>
        public async Task PublishEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Draft)
                throw new InvalidEventStateException(id, $"Only draft events can be published. Current status: {eventEntity.Status}");

            eventEntity.Publish();

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Activate event: Upcoming → Active (event is currently in progress)
        /// </summary>
        public async Task ActivateEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Upcoming)
                throw new InvalidEventStateException(id, $"Only upcoming events can be activated. Current status: {eventEntity.Status}");

            eventEntity.Activate();

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Complete event: Upcoming or Active → Completed (event is finished, ready to award points)
        /// </summary>
        public async Task CompleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Upcoming && eventEntity.Status != EventStatus.Active)
                throw new InvalidEventStateException(id, $"Only upcoming or active events can be completed. Current status: {eventEntity.Status}");

            eventEntity.Complete();

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Revert to draft: DISABLED - One-way flow only (Draft → Upcoming → Active → Completed)
        /// This method now throws an error to enforce one-way status transitions.
        /// </summary>
        [Obsolete("Backward status transitions are no longer allowed. Events can only move forward.")]
        public async Task RevertToDraftAsync(Guid id)
        {
            throw new InvalidOperationException(
                "Backward status transitions are not allowed. Events can only move forward: " +
                "Draft → Upcoming → Active → Completed.");
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            // Delete event - we'll just remove it from the database
            await _unitOfWork.Events.DeleteAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}