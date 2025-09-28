using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Events;

namespace RewardPointsSystem.Services.Events
{
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
                throw new ArgumentException("Points pool must be greater than zero", nameof(pointsPool));

            var eventEntity = new Event
            {
                Name = name,
                Description = description,
                StartDate = date,
                EndDate = date.AddHours(4),
                PointsReward = pointsPool,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Events.AddAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
            return eventEntity;
        }

        public async Task<Event> UpdateEventAsync(Guid id, EventUpdateDto updates)
        {
            if (updates == null)
                throw new ArgumentNullException(nameof(updates));

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {id} not found");

            if (!string.IsNullOrWhiteSpace(updates.Name))
                eventEntity.Name = updates.Name;

            if (!string.IsNullOrWhiteSpace(updates.Description))
                eventEntity.Description = updates.Description;

            if (updates.EventDate.HasValue)
            {
                eventEntity.StartDate = updates.EventDate.Value;
                eventEntity.EndDate = updates.EventDate.Value.AddHours(4);
            }

            if (updates.TotalPointsPool.HasValue)
                eventEntity.PointsReward = updates.TotalPointsPool.Value;

            eventEntity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Events.UpdateAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
            return eventEntity;
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _unitOfWork.Events.FindAsync(e => e.IsActive && e.StartDate > currentDate);
        }

        public async Task<IEnumerable<Event>> GetActiveEventsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _unitOfWork.Events.FindAsync(e => e.IsActive && e.StartDate <= currentDate && e.EndDate >= currentDate);
        }

        public async Task<Event> GetEventByIdAsync(Guid id)
        {
            return await _unitOfWork.Events.GetByIdAsync(id);
        }

        public async Task CompleteEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {id} not found");

            eventEntity.EndDate = DateTime.UtcNow;
            eventEntity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Events.UpdateAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {id} not found");

            eventEntity.IsActive = false;
            eventEntity.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Events.UpdateAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}