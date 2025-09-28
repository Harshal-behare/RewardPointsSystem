using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Events;
using RewardPointsSystem.DTOs;

namespace RewardPointsSystem.Services.Events
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
                throw new ArgumentException("Event name is required", nameof(name));
            
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Event description is required", nameof(description));
            
            if (pointsPool <= 0)
                throw new ArgumentException("Points pool must be positive", nameof(pointsPool));
            
            if (date < DateTime.UtcNow.Date)
                throw new ArgumentException("Cannot create events in the past");

            var eventEntity = new Event
            {
                Name = name.Trim(),
                Description = description.Trim(),
                EventDate = date,
                TotalPointsPool = pointsPool,
                Status = EventStatus.Upcoming,
                CreatedBy = Guid.Empty, // TODO: Get from current user context
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Events.AddAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
            
            return eventEntity;
        }

        public async Task<Event> UpdateEventAsync(Guid id, UpdateEventDto updates)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new ArgumentException($"Event with ID {id} not found", nameof(id));

            if (eventEntity.Status == EventStatus.Completed || eventEntity.Status == EventStatus.Cancelled)
                throw new InvalidOperationException("Cannot modify completed or cancelled events");

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

        public async Task CompleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new ArgumentException($"Event with ID {id} not found", nameof(id));

            if (eventEntity.Status != EventStatus.Active)
                throw new InvalidOperationException($"Only active events can be completed. Current status: {eventEntity.Status}");

            eventEntity.Status = EventStatus.Completed;
            eventEntity.CompletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new ArgumentException($"Event with ID {id} not found", nameof(id));

            if (eventEntity.Status == EventStatus.Completed)
                throw new InvalidOperationException("Cannot cancel completed events");

            eventEntity.Status = EventStatus.Cancelled;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}