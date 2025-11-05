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
            
            if (date < DateTime.UtcNow.Date)
                throw new InvalidEventDataException("Cannot create events in the past");

            // Get or create system user for event creation
            var systemUser = await GetOrCreateSystemUserAsync();

            var eventEntity = new Event
            {
                Name = name.Trim(),
                Description = description.Trim(),
                EventDate = date,
                TotalPointsPool = pointsPool,
                Status = EventStatus.Upcoming,
                CreatedBy = systemUser.Id,
                CreatedAt = DateTime.UtcNow
            };

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
                systemUser = new User
                {
                    Email = "system@rewardpoints.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
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

            if (!string.IsNullOrWhiteSpace(updates.Name))
                eventEntity.Name = updates.Name.Trim();
            
            if (!string.IsNullOrWhiteSpace(updates.Description))
                eventEntity.Description = updates.Description.Trim();
            
            if (updates.EventDate != default)
                eventEntity.EventDate = updates.EventDate;
            
            if (updates.TotalPointsPool > 0)
                eventEntity.TotalPointsPool = updates.TotalPointsPool;

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

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _unitOfWork.Events.GetByIdAsync(id);
        }

        public async Task ActivateEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Upcoming)
                throw new InvalidEventStateException(id, $"Only upcoming events can be activated. Current status: {eventEntity.Status}");

            eventEntity.Status = EventStatus.Active;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CompleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status != EventStatus.Active)
                throw new InvalidEventStateException(id, $"Only active events can be completed. Current status: {eventEntity.Status}");

            eventEntity.Status = EventStatus.Completed;
            eventEntity.CompletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new EventNotFoundException(id);

            if (eventEntity.Status == EventStatus.Completed)
                throw new InvalidEventStateException(id, "Cannot cancel completed events");

            eventEntity.Status = EventStatus.Cancelled;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}