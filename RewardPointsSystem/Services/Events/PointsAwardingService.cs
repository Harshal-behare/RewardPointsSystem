using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Events;

namespace RewardPointsSystem.Services.Events
{
    public class PointsAwardingService : IPointsAwardingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PointsAwardingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<EventParticipant> AwardPointsAsync(Guid eventId, Guid userId, decimal points, int position)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            if (position <= 0)
                throw new ArgumentException("Position must be greater than zero", nameof(position));

            // Validate event exists
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            // Validate user participated in the event
            var participation = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(
                ep => ep.EventId == eventId && ep.UserId == userId);
            if (participation == null)
                throw new InvalidOperationException($"User {userId} did not participate in event {eventId}");

            // Check if points already awarded
            if (participation.PointsAwarded.HasValue)
                throw new InvalidOperationException($"Points already awarded to user {userId} for event {eventId}");

            // Validate user exists and is active
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            // Check if there's sufficient points in the event pool
            var totalAwarded = await GetTotalPointsAwardedAsync(eventId);
            if (totalAwarded + points > eventEntity.PointsReward)
                throw new InvalidOperationException($"Insufficient points in event pool. Available: {eventEntity.PointsReward - totalAwarded}, Requested: {points}");

            // Award points
            participation.PointsAwarded = points;
            participation.Position = position;
            participation.AwardedAt = DateTime.UtcNow;

            await _unitOfWork.EventParticipants.UpdateAsync(participation);
            await _unitOfWork.SaveChangesAsync();

            return participation;
        }

        public async Task<IEnumerable<EventParticipant>> BulkAwardPointsAsync(Guid eventId, IEnumerable<WinnerDto> winners)
        {
            if (winners == null || !winners.Any())
                throw new ArgumentException("Winners list cannot be empty", nameof(winners));

            var winnersList = winners.ToList();

            // Validate event exists
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            // Check total points required
            var totalPointsRequired = winnersList.Sum(w => w.Points);
            var totalAlreadyAwarded = await GetTotalPointsAwardedAsync(eventId);

            if (totalAlreadyAwarded + totalPointsRequired > eventEntity.PointsReward)
                throw new InvalidOperationException($"Insufficient points in event pool. Available: {eventEntity.PointsReward - totalAlreadyAwarded}, Requested: {totalPointsRequired}");

            var updatedParticipants = new List<EventParticipant>();

            foreach (var winner in winnersList)
            {
                var participant = await AwardPointsAsync(eventId, winner.UserId, winner.Points, winner.Position);
                updatedParticipants.Add(participant);
            }

            return updatedParticipants;
        }

        public async Task<bool> HasUserBeenAwardedAsync(Guid eventId, Guid userId)
        {
            var participation = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(
                ep => ep.EventId == eventId && ep.UserId == userId);

            return participation?.PointsAwarded.HasValue == true;
        }

        public async Task<decimal> GetRemainingPointsPoolAsync(Guid eventId)
        {
            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            var totalAwarded = await GetTotalPointsAwardedAsync(eventId);
            return eventEntity.PointsReward - totalAwarded;
        }

        public async Task<decimal> GetTotalPointsAwardedAsync(Guid eventId)
        {
            var participants = await _unitOfWork.EventParticipants.FindAsync(
                ep => ep.EventId == eventId && ep.PointsAwarded.HasValue);

            return participants.Sum(p => p.PointsAwarded.Value);
        }

        public async Task<IEnumerable<EventParticipant>> GetEventWinnersAsync(Guid eventId)
        {
            return await _unitOfWork.EventParticipants.FindAsync(
                ep => ep.EventId == eventId && ep.PointsAwarded.HasValue && ep.Position.HasValue);
        }

        public async Task<bool> CanAwardPointsAsync(Guid eventId, decimal points)
        {
            var remainingPoints = await GetRemainingPointsPoolAsync(eventId);
            return remainingPoints >= points;
        }
    }
}