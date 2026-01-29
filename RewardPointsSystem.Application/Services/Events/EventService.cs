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
            
            if (pointsPool <= 0)
                throw new InvalidEventDataException("Points pool must be positive");
            
            // Only validate future date for creation
            if (date < DateTime.UtcNow.Date)
                throw new InvalidEventDataException("Cannot create events in the past");

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
            var pointsPool = updates.TotalPointsPool ?? eventEntity.TotalPointsPool;
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

            // Handle status change if provided
            if (!string.IsNullOrWhiteSpace(updates.Status))
            {
                var targetStatus = updates.Status.ToLower();
                var currentStatus = eventEntity.Status;

                // Apply status transitions based on target status
                switch (targetStatus)
                {
                    case "draft":
                        if (currentStatus == EventStatus.Upcoming)
                            eventEntity.RevertToDraft();
                        break;
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
            return events;
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            // Include participants so we can count them
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => true,  // All events
                e => e.Participants);
            return events;
        }

        /// <summary>
        /// Get visible events for employees (Upcoming, Active, Completed - excludes Draft)
        /// </summary>
        public async Task<IEnumerable<Event>> GetVisibleEventsAsync()
        {
            // Include participants so we can count them
            // Return events visible to employees: Upcoming, Active, Completed (not Draft)
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => e.Status != EventStatus.Draft,
                e => e.Participants);
            return events;
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            // Include participants for single event
            var events = await _unitOfWork.Events.FindWithIncludesAsync(
                e => e.Id == id,
                e => e.Participants);
            return events.FirstOrDefault();
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
        /// Revert to draft: Upcoming → Draft (hide event from employees)
        /// </summary>
        public async Task RevertToDraftAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Upcoming)
                throw new InvalidEventStateException(id, $"Only upcoming events can be reverted to draft. Current status: {eventEntity.Status}");

            eventEntity.RevertToDraft();

            await _unitOfWork.SaveChangesAsync();
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