using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;

namespace RewardPointsSystem.Application.Services.Events
{
    public class EventParticipationService : IEventParticipationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventParticipationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task RegisterParticipantAsync(Guid eventId, Guid userId)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null || eventEntity.Status == EventStatus.Cancelled)
                throw new InvalidOperationException($"Event with ID {eventId} not found or inactive");

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            var existingParticipation = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
            if (existingParticipation != null)
                throw new InvalidOperationException($"User is already registered for this event");

            var participant = EventParticipant.Register(eventId, userId);

            await _unitOfWork.EventParticipants.AddAsync(participant);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(Guid eventId)
        {
            return await _unitOfWork.EventParticipants.FindAsync(ep => ep.EventId == eventId);
        }

        public async Task<IEnumerable<EventParticipant>> GetUserEventsAsync(Guid userId)
        {
            return await _unitOfWork.EventParticipants.FindAsync(ep => ep.UserId == userId);
        }

        public async Task RemoveParticipantAsync(Guid eventId, Guid userId)
        {
            var participant = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
            if (participant == null)
                throw new InvalidOperationException($"User is not registered for this event");

            await _unitOfWork.EventParticipants.DeleteAsync(participant);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsUserRegisteredAsync(Guid eventId, Guid userId)
        {
            return await _unitOfWork.EventParticipants.ExistsAsync(ep => ep.EventId == eventId && ep.UserId == userId);
        }
    }
}