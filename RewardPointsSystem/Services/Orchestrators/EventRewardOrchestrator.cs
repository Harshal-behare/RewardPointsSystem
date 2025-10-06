using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Events;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Services.Orchestrators
{
    /// <summary>
    /// Service: EventRewardOrchestrator
    /// Responsibility: Coordinate event reward flow only
    /// </summary>
    public class EventRewardOrchestrator : IEventRewardOrchestrator
    {
        private readonly IEventService _eventService;
        private readonly IEventParticipationService _participationService;
        private readonly IPointsAwardingService _pointsAwardingService;
        private readonly IPointsAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public EventRewardOrchestrator(
            IEventService eventService,
            IEventParticipationService participationService,
            IPointsAwardingService pointsAwardingService,
            IPointsAccountService accountService,
            ITransactionService transactionService)
        {
            _eventService = eventService;
            _participationService = participationService;
            _pointsAwardingService = pointsAwardingService;
            _accountService = accountService;
            _transactionService = transactionService;
        }

        public async Task<EventRewardResult> ProcessEventRewardAsync(Guid eventId, Guid userId, int points, int position, Guid awardedBy)
        {
            try
            {
                // 1. Validate event status (EventService)
                var eventObj = await _eventService.GetEventByIdAsync(eventId);
                if (eventObj == null)
                    throw new ArgumentException($"Event with ID {eventId} not found");

                if (eventObj.Status != EventStatus.Active && eventObj.Status != EventStatus.Completed)
                    throw new InvalidOperationException($"Event must be Active or Completed to award points. Current status: {eventObj.Status}");

                // 2. Verify participation (EventParticipationService)
                var isRegistered = await _participationService.IsUserRegisteredAsync(eventId, userId);
                if (!isRegistered)
                    throw new InvalidOperationException($"User {userId} is not registered for event {eventId}");

                // 3. Check points pool (PointsAwardingService)
                var remainingPoints = await _pointsAwardingService.GetRemainingPointsPoolAsync(eventId);
                if (remainingPoints < points)
                    throw new InvalidOperationException($"Insufficient points in pool. Remaining: {remainingPoints}, Requested: {points}");

                // Check if user already awarded
                var hasBeenAwarded = await _pointsAwardingService.HasUserBeenAwardedAsync(eventId, userId);
                if (hasBeenAwarded)
                    throw new InvalidOperationException($"User {userId} has already been awarded points for event {eventId}");

                // 4. Award points (PointsAwardingService)
                await _pointsAwardingService.AwardPointsAsync(eventId, userId, points, position);

                // 5. Update balance (PointsAccountService)
                // Ensure account exists
                var account = await _accountService.GetAccountAsync(userId);
                if (account == null)
                    await _accountService.CreateAccountAsync(userId);

                await _accountService.AddPointsAsync(userId, points);

                // 6. Record transaction (TransactionService)
                await _transactionService.RecordEarnedPointsAsync(userId, points, eventId, 
                    $"Event reward - Position {position} in {eventObj.Name}");

                // Get updated participation record
                var participants = await _participationService.GetEventParticipantsAsync(eventId);
                var participation = participants.FirstOrDefault(p => p.UserId == userId);

                // Get transaction record
                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                var transaction = transactions.OrderByDescending(t => t.Timestamp).FirstOrDefault();

                return new EventRewardResult
                {
                    Success = true,
                    Message = $"Successfully awarded {points} points for position {position} in {eventObj.Name}",
                    EventName = eventObj.Name,
                    Participation = participation,
                    Transaction = transaction
                };
            }
            catch (Exception ex)
            {
                return new EventRewardResult
                {
                    Success = false,
                    Message = $"Failed to process event reward: {ex.Message}",
                    EventName = null,
                    Participation = null,
                    Transaction = null
                };
            }
        }
    }
}