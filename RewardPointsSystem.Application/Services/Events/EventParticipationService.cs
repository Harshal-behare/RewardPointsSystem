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
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            // Check event status - only Upcoming events accept registrations
            if (eventEntity.Status == EventStatus.Draft)
                throw new InvalidOperationException("Event is not yet published for registration");

            if (eventEntity.Status == EventStatus.Completed)
                throw new InvalidOperationException("Event has already ended");

            if (eventEntity.Status == EventStatus.Active)
                throw new InvalidOperationException("Event is currently in progress, registration closed");

            // Validate registration window (per System.txt requirements)
            var now = DateTime.UtcNow;

            if (eventEntity.RegistrationStartDate.HasValue && now < eventEntity.RegistrationStartDate.Value)
                throw new InvalidOperationException("Registration has not started yet");

            if (eventEntity.RegistrationEndDate.HasValue && now > eventEntity.RegistrationEndDate.Value)
                throw new InvalidOperationException("Registration period has ended");

            // Validate user
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            if (!user.IsActive)
                throw new InvalidOperationException("Your account is inactive. Please contact administrator.");

            // Check if already registered
            var existingParticipation = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
            if (existingParticipation != null)
                throw new InvalidOperationException("You are already registered for this event");

            // Check max participants limit
            if (eventEntity.MaxParticipants.HasValue)
            {
                var currentCount = await _unitOfWork.EventParticipants.CountAsync(ep => ep.EventId == eventId);
                if (currentCount >= eventEntity.MaxParticipants.Value)
                    throw new InvalidOperationException("Event is full. Maximum participants reached.");
            }

            var participant = EventParticipant.Register(eventId, userId);

            await _unitOfWork.EventParticipants.AddAsync(participant);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(Guid eventId)
        {
            return await _unitOfWork.EventParticipants.FindWithIncludesAsync(
                ep => ep.EventId == eventId,
                ep => ep.User);
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