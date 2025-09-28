using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Events;

namespace RewardPointsSystem.Services.Events
{
    public class EventParticipationService : IEventParticipationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventParticipationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<EventParticipant> RegisterParticipantAsync(Guid eventId, Guid userId)
        {
            // Validate event exists and is active
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null || !eventEntity.IsActive)
                throw new InvalidOperationException($"Event with ID {eventId} not found or inactive");

            // Check if event is still open for registration
            var currentDate = DateTime.UtcNow;
            if (eventEntity.StartDate <= currentDate)
                throw new InvalidOperationException("Cannot register for an event that has already started");

            // Validate user exists and is active
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            // Check if user is already registered
            var existingParticipation = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(
                ep => ep.EventId == eventId && ep.UserId == userId);
            if (existingParticipation != null)
                throw new InvalidOperationException($"User {userId} is already registered for event {eventId}");

            // Check event capacity
            if (eventEntity.MaxParticipants.HasValue)
            {
                var currentParticipants = await _unitOfWork.EventParticipants.CountAsync(ep => ep.EventId == eventId);
                if (currentParticipants >= eventEntity.MaxParticipants.Value)
                    throw new InvalidOperationException($"Event {eventId} has reached maximum capacity");
            }

            var participant = new EventParticipant
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = userId,
                ParticipatedAt = DateTime.UtcNow,
                PointsAwarded = null, // Will be set when points are awarded
                Position = null // Will be set when results are recorded
            };

            await _unitOfWork.EventParticipants.AddAsync(participant);
            await _unitOfWork.SaveChangesAsync();

            return participant;
        }

        public async Task RemoveParticipantAsync(Guid eventId, Guid userId)
        {
            // Validate event exists and hasn't started
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            var currentDate = DateTime.UtcNow;
            if (eventEntity.StartDate <= currentDate)
                throw new InvalidOperationException("Cannot remove participant from an event that has already started");

            var participant = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(
                ep => ep.EventId == eventId && ep.UserId == userId);
            if (participant == null)
                throw new InvalidOperationException($"User {userId} is not registered for event {eventId}");

            await _unitOfWork.EventParticipants.DeleteAsync(participant);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(Guid eventId)
        {
            return await _unitOfWork.EventParticipants.FindAsync(ep => ep.EventId == eventId);
        }

        public async Task<IEnumerable<EventParticipant>> GetUserEventParticipationsAsync(Guid userId)
        {
            return await _unitOfWork.EventParticipants.FindAsync(ep => ep.UserId == userId);
        }

        public async Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId)
        {
            return await _unitOfWork.EventParticipants.ExistsAsync(ep => ep.EventId == eventId && ep.UserId == userId);
        }

        public async Task<int> GetParticipantCountAsync(Guid eventId)
        {
            return await _unitOfWork.EventParticipants.CountAsync(ep => ep.EventId == eventId);
        }

        public async Task<bool> HasUserWonAsync(Guid eventId, Guid userId)
        {
            var participation = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(
                ep => ep.EventId == eventId && ep.UserId == userId);

            return participation?.PointsAwarded != null && participation.PointsAwarded > 0;
        }
    }
}