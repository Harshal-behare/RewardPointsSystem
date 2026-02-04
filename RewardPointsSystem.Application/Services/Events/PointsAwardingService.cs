using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Exceptions;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Services.Events
{
    public class PointsAwardingService : IPointsAwardingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminBudgetService _budgetService;

        public PointsAwardingService(IUnitOfWork unitOfWork, IAdminBudgetService budgetService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _budgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
        }

        public async Task AwardPointsAsync(Guid userId, int points, string description, Guid? eventId = null, Guid? awardingAdminId = null)
        {
            // Simple award points without event context
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            // Validate and track admin budget if admin ID is provided
            if (awardingAdminId.HasValue)
            {
                var budgetValidation = await _budgetService.ValidatePointsAwardAsync(awardingAdminId.Value, points);
                if (!budgetValidation.IsAllowed)
                {
                    throw new InvalidOperationException(budgetValidation.Message ?? "Budget validation failed");
                }
            }

            // Verify user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);

            if (!user.IsActive)
                throw new InactiveUserException(userId);

            // If event ID is provided, update the participant record to mark points as awarded
            if (eventId.HasValue)
            {
                var participant = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(
                    ep => ep.EventId == eventId.Value && ep.UserId == userId);
                    
                if (participant != null)
                {
                    // Check if already awarded
                    if (participant.PointsAwarded.HasValue)
                        throw new InvalidOperationException($"Points already awarded to this user for this event");
                    
                    // Check remaining pool
                    var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId.Value);
                    if (eventEntity != null)
                    {
                        var totalAwarded = await GetTotalPointsAwardedAsync(eventId.Value);
                        if (totalAwarded + points > eventEntity.TotalPointsPool)
                            throw new InvalidOperationException($"Not enough points remaining in pool");
                    }
                    
                    // Auto check-in if only registered
                    if (participant.AttendanceStatus == AttendanceStatus.Registered)
                    {
                        participant.CheckIn();
                    }
                    
                    // Award points to participant record
                    participant.AwardPoints(points, 1, eventEntity?.CreatedBy ?? Guid.Empty);
                    await _unitOfWork.EventParticipants.UpdateAsync(participant);
                }
            }

            // Get or create points account
            var account = await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(a => a.UserId == userId);
            bool isNewAccount = false;
            if (account == null)
            {
                // Auto-create points account if it doesn't exist
                account = UserPointsAccount.CreateForUser(userId);
                await _unitOfWork.UserPointsAccounts.AddAsync(account);
                isNewAccount = true;
            }

            // Credit points to account (pass null as no specific admin user tracked)
            account.CreditPoints(points, null);
            
            // Only call UpdateAsync if the account already existed
            if (!isNewAccount)
            {
                await _unitOfWork.UserPointsAccounts.UpdateAsync(account);
            }

            // Create transaction record for admin award
            var sourceId = eventId ?? Guid.NewGuid(); // Use eventId if provided, otherwise generate new ID for admin award
            var transactionOrigin = eventId.HasValue ? TransactionOrigin.Event : TransactionOrigin.AdminAward;
            var transactionDescription = string.IsNullOrWhiteSpace(description) ? "Points awarded by admin" : description;
            
            var transaction = UserPointsTransaction.CreateEarned(
                userId,
                points,
                transactionOrigin,
                sourceId,
                account.CurrentBalance,
                transactionDescription);

            await _unitOfWork.UserPointsTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Record points against admin's monthly budget
            if (awardingAdminId.HasValue)
            {
                await _budgetService.RecordPointsAwardedAsync(awardingAdminId.Value, points);
            }
        }

        public async Task AwardPointsAsync(Guid eventId, Guid userId, int points, int eventRank, Guid? awardingAdminId = null)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            var eventEntity = await _unitOfWork.Events.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new InvalidOperationException($"Event with ID {eventId} not found");

            var participant = await _unitOfWork.EventParticipants.SingleOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
            if (participant == null)
                throw new InvalidOperationException($"User did not participate in this event");

            if (participant.AttendanceStatus == AttendanceStatus.Registered)
            {
                participant.CheckIn();
            }

            if (participant.PointsAwarded.HasValue)
                throw new InvalidOperationException($"Points already awarded to this user for this event");

            var totalAwarded = await GetTotalPointsAwardedAsync(eventId);
            if (totalAwarded + points > eventEntity.TotalPointsPool)
                throw new InvalidOperationException($"Not enough points remaining in pool");

            // Validate and track admin budget if admin ID is provided
            if (awardingAdminId.HasValue)
            {
                var budgetValidation = await _budgetService.ValidatePointsAwardAsync(awardingAdminId.Value, points);
                if (!budgetValidation.IsAllowed)
                {
                    throw new InvalidOperationException(budgetValidation.Message ?? "Budget validation failed");
                }
            }

            participant.AwardPoints(points, eventRank, eventEntity.CreatedBy);

            await _unitOfWork.EventParticipants.UpdateAsync(participant);
            await _unitOfWork.SaveChangesAsync();

            // Record points against admin's monthly budget
            if (awardingAdminId.HasValue)
            {
                await _budgetService.RecordPointsAwardedAsync(awardingAdminId.Value, points);
            }
        }

        public async Task BulkAwardPointsAsync(Guid eventId, List<WinnerDto> winners, Guid? awardingAdminId = null)
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

            // Validate admin budget upfront for total points
            if (awardingAdminId.HasValue)
            {
                var budgetValidation = await _budgetService.ValidatePointsAwardAsync(awardingAdminId.Value, totalPointsRequired);
                if (!budgetValidation.IsAllowed)
                {
                    throw new InvalidOperationException(budgetValidation.Message ?? "Budget validation failed");
                }
            }

            foreach (var winner in winners)
            {
                await AwardPointsAsync(eventId, winner.UserId, winner.Points, winner.EventRank, awardingAdminId);
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
