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
        private readonly IRewardAccountService _accountService;
        private readonly ITransactionService _transactionService;

        public EventRewardOrchestrator(
            IEventService eventService,
            IEventParticipationService participationService,
            IPointsAwardingService pointsAwardingService,
            IRewardAccountService accountService,
            ITransactionService transactionService)
        {
            _eventService = eventService;
            _participationService = participationService;
            _pointsAwardingService = pointsAwardingService;
            _accountService = accountService;
            _transactionService = transactionService;
        }

        public async Task<EventParticipationResult> ProcessEventParticipationAsync(Guid eventId, Guid userId)
        {
            try
            {
                // 1. Validate event eligibility
                var isEligible = await ValidateEventEligibilityAsync(eventId, userId);
                if (!isEligible)
                    throw new InvalidOperationException($"User {userId} is not eligible for event {eventId}");

                // 2. Register participant
                await _participationService.RegisterParticipantAsync(eventId, userId);

                // 3. Get the participation record
                var participants = await _participationService.GetEventParticipantsAsync(eventId);
                var participation = participants.FirstOrDefault(p => p.UserId == userId && p.EventId == eventId);

                return new EventParticipationResult
                {
                    Success = true,
                    Message = "Successfully registered for event",
                    Participation = participation,
                    Transaction = null // No transaction until points are awarded
                };
            }
            catch (Exception ex)
            {
                return new EventParticipationResult
                {
                    Success = false,
                    Message = $"Failed to process event participation: {ex.Message}",
                    Participation = null,
                    Transaction = null
                };
            }
        }

        public async Task<IEnumerable<PointsTransaction>> ProcessBulkEventParticipationAsync(Guid eventId, IEnumerable<Guid> userIds)
        {
            var transactions = new List<PointsTransaction>();

            foreach (var userId in userIds)
            {
                try
                {
                    var result = await ProcessEventParticipationAsync(eventId, userId);
                    if (result.Success && result.Transaction != null)
                    {
                        transactions.Add(result.Transaction);
                    }
                }
                catch
                {
                    // Continue with other users even if one fails
                    continue;
                }
            }

            return transactions;
        }

        public async Task<EventRewardSummary> GetEventRewardSummaryAsync(Guid eventId)
        {
            var eventObj = await _eventService.GetEventByIdAsync(eventId);
            var participants = await _participationService.GetEventParticipantsAsync(eventId);
            
            var totalPointsAwarded = participants
                .Where(p => p.PointsAwarded.HasValue)
                .Sum(p => p.PointsAwarded.Value);

            return new EventRewardSummary
            {
                EventId = eventId,
                EventName = eventObj?.Name ?? "Unknown Event",
                TotalParticipants = participants.Count(),
                TotalPointsAwarded = totalPointsAwarded,
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> ValidateEventEligibilityAsync(Guid eventId, Guid userId)
        {
            try
            {
                // Check if event exists and is active
                var eventObj = await _eventService.GetEventByIdAsync(eventId);
                if (eventObj == null)
                    return false;

                if (eventObj.Status != EventStatus.Upcoming && eventObj.Status != EventStatus.Active)
                    return false;

                // Check if user is already registered
                var isAlreadyRegistered = await _participationService.IsUserRegisteredAsync(eventId, userId);
                if (isAlreadyRegistered)
                    return false; // Already registered

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}