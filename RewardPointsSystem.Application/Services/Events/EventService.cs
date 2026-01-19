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
    /// Architecture Compliant - SRP
    /// </summary>
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Event> CreateEventAsync(string name, string description, DateTime date, int pointsPool)
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
                description.Trim());

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

            if (eventEntity.Status == EventStatus.Completed || eventEntity.Status == EventStatus.Cancelled)
                throw new InvalidEventStateException(id, "Cannot modify completed or cancelled events");

            var name = !string.IsNullOrWhiteSpace(updates.Name) ? updates.Name.Trim() : eventEntity.Name;
            var description = !string.IsNullOrWhiteSpace(updates.Description) ? updates.Description.Trim() : eventEntity.Description;
            var eventDate = updates.EventDate ?? eventEntity.EventDate;
            var pointsPool = updates.TotalPointsPool ?? eventEntity.TotalPointsPool;

            eventEntity.UpdateDetails(name, eventDate, pointsPool, description);

            await _unitOfWork.SaveChangesAsync();
            return eventEntity;
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            var events = await _unitOfWork.Events.GetAllAsync();
            return events.Where(e => e.Status == EventStatus.Upcoming);
        }

        public async Task<IEnumerable<Event>> GetActiveEventsAsync()
        {
            var events = await _unitOfWork.Events.GetAllAsync();
            return events.Where(e => e.Status == EventStatus.Active);
        }

        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            return await _unitOfWork.Events.GetAllAsync();
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _unitOfWork.Events.GetByIdAsync(id);
        }

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

        public async Task ActivateEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Upcoming)
                throw new InvalidEventStateException(id, $"Only upcoming events can be activated. Current status: {eventEntity.Status}");

            eventEntity.Start();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CompleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Active)
                throw new InvalidEventStateException(id, $"Only active events can be completed. Current status: {eventEntity.Status}");

            eventEntity.Complete();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status == EventStatus.Completed)
                throw new InvalidEventStateException(id, "Cannot cancel completed events");

            eventEntity.Cancel();

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            eventEntity.Delete();

            await _unitOfWork.SaveChangesAsync();
        }
    }
}