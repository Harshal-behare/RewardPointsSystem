using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Services.Events
{
    public class PointsAwardingService : IPointsAwardingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PointsAwardingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task AwardPointsAsync(Guid eventId, Guid userId, int points, int eventRank)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            var participant = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
            if (participant == null)
                throw new InvalidOperationException($"User did not participate in this event");

            if (participant.PointsAwarded.HasValue)
                throw new InvalidOperationException($"Points already awarded to this user for this event");

            var totalAwarded = await GetTotalPointsAwardedAsync(eventId);
            if (totalAwarded + points > eventEntity.TotalPointsPool)
                throw new InvalidOperationException($"Not enough points remaining in pool");

            participant.PointsAwarded = points;
            participant.EventRank = eventRank;
            participant.AwardedAt = DateTime.UtcNow;

            await _unitOfWork.EventParticipants.UpdateAsync(participant);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task BulkAwardPointsAsync(Guid eventId, List<WinnerDto> winners)
        {
            if (winners == null || !winners.Any())
                throw new ArgumentException("Winners list cannot be empty", nameof(winners));

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            var totalPointsRequired = winners.Sum(w => w.Points);
            var totalAwarded = await GetTotalPointsAwardedAsync(eventId);

            if (totalAwarded + totalPointsRequired > eventEntity.TotalPointsPool)
                throw new InvalidOperationException($"Not enough points remaining in pool");

            foreach (var winner in winners)
            {
                await AwardPointsAsync(eventId, winner.UserId, winner.Points, winner.EventRank);
            }
        }

        public async Task<bool> HasUserBeenAwardedAsync(Guid eventId, Guid userId)
        {
            var participant = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
            return participant?.PointsAwarded.HasValue == true;
        }

        public async Task<int> GetRemainingPointsPoolAsync(Guid eventId)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            var totalAwarded = await GetTotalPointsAwardedAsync(eventId);
            return eventEntity.TotalPointsPool - totalAwarded;
        }

        private async Task<int> GetTotalPointsAwardedAsync(Guid eventId)
        {
            var participants = await _unitOfWork.EventParticipants.FindAsync(ep => ep.EventId == eventId && ep.PointsAwarded.HasValue);
            return participants.Sum(p => p.PointsAwarded.Value);
        }
    }
}