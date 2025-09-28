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

        public async Task<Event> CreateEventAsync(CreateEventDto eventDto)
        {
            if (eventDto == null)
                throw new ArgumentNullException(nameof(eventDto));

            if (string.IsNullOrWhiteSpace(eventDto.Name))
                throw new ArgumentException("Event name is required", nameof(eventDto));

            if (string.IsNullOrWhiteSpace(eventDto.Description))
                throw new ArgumentException("Event description is required", nameof(eventDto));

            if (eventDto.StartDate >= eventDto.EndDate)
                throw new ArgumentException("Event start date must be before end date", nameof(eventDto));

            if (eventDto.PointsReward <= 0)
                throw new ArgumentException("Points reward must be greater than zero", nameof(eventDto));

            var eventEntity = new Event
            {
                Id = Guid.NewGuid(),
                Name = eventDto.Name,
                Description = eventDto.Description,
                StartDate = eventDto.StartDate,
                EndDate = eventDto.EndDate,
                PointsReward = eventDto.PointsReward,
                MaxParticipants = eventDto.MaxParticipants,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Events.AddAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            return eventEntity;
        }

        public async Task<Event> UpdateEventAsync(Guid id, UpdateEventDto updates)
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

            if (updates.StartDate.HasValue && updates.EndDate.HasValue)
            {
                if (updates.StartDate.Value >= updates.EndDate.Value)
                    throw new ArgumentException("Event start date must be before end date");

                eventEntity.StartDate = updates.StartDate.Value;
                eventEntity.EndDate = updates.EndDate.Value;
            }
            else if (updates.StartDate.HasValue)
            {
                if (updates.StartDate.Value >= eventEntity.EndDate)
                    throw new ArgumentException("Event start date must be before end date");
                
                eventEntity.StartDate = updates.StartDate.Value;
            }
            else if (updates.EndDate.HasValue)
            {
                if (eventEntity.StartDate >= updates.EndDate.Value)
                    throw new ArgumentException("Event start date must be before end date");
                
                eventEntity.EndDate = updates.EndDate.Value;
            }

            if (updates.PointsReward.HasValue)
            {
                if (updates.PointsReward.Value <= 0)
                    throw new ArgumentException("Points reward must be greater than zero");
                
                eventEntity.PointsReward = updates.PointsReward.Value;
            }

            if (updates.MaxParticipants.HasValue)
                eventEntity.MaxParticipants = updates.MaxParticipants;

            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();

            return eventEntity;
        }

        public async Task<Event> GetEventAsync(Guid id)
        {
            return await _unitOfWork.Events.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Event>> GetActiveEventsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _unitOfWork.Events.FindAsync(e => e.IsActive && e.StartDate <= currentDate && e.EndDate >= currentDate);
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _unitOfWork.Events.FindAsync(e => e.IsActive && e.StartDate > currentDate);
        }

        public async Task<IEnumerable<Event>> GetPastEventsAsync()
        {
            var currentDate = DateTime.UtcNow;
            return await _unitOfWork.Events.FindAsync(e => e.EndDate < currentDate);
        }

        public async Task DeactivateEventAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {id} not found");

            eventEntity.IsActive = false;
            eventEntity.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Events.UpdateAsync(eventEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsEventActiveAsync(Guid id)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(id);
            if (eventEntity == null)
                return false;

            var currentDate = DateTime.UtcNow;
            return eventEntity.IsActive && eventEntity.StartDate <= currentDate && eventEntity.EndDate >= currentDate;
        }

        public async Task<int> GetParticipantCountAsync(Guid eventId)
        {
            return await _unitOfWork.EventParticipants.CountAsync(ep => ep.EventId == eventId);
        }

        public async Task<bool> HasCapacityAsync(Guid eventId)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity?.MaxParticipants == null)
                return true;

            var participantCount = await GetParticipantCountAsync(eventId);
            return participantCount < eventEntity.MaxParticipants.Value;
        }
    }
}