using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Services.Accounts
{
    public class UserPointsTransactionService : IUserPointsTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserPointsTransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task RecordEarnedUserPointsAsync(Guid userId, int userPoints, Guid eventId, string description)
        {
            if (userPoints <= 0)
                throw new ArgumentException("User points must be greater than zero", nameof(userPoints));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            // Get current balance to calculate balance after
            var account = await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                throw new InvalidOperationException($"User points account not found for user {userId}");

            var balanceAfter = account.CurrentBalance + userPoints;

            var transaction = new UserPointsTransaction
            {
                UserId = userId,
                UserPoints = userPoints,
                TransactionType = TransactionCategory.Earned,
                TransactionSource = TransactionOrigin.Event,
                SourceId = eventId,
                Description = description,
                Timestamp = DateTime.UtcNow,
                BalanceAfter = balanceAfter
            };

            await _unitOfWork.UserPointsTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RecordRedeemedUserPointsAsync(Guid userId, int userPoints, Guid redemptionId, string description)
        {
            if (userPoints <= 0)
                throw new ArgumentException("User points must be greater than zero", nameof(userPoints));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            // Get current balance to calculate balance after
            var account = await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                throw new InvalidOperationException($"User points account not found for user {userId}");

            var balanceAfter = account.CurrentBalance - userPoints;

            var transaction = new UserPointsTransaction
            {
                UserId = userId,
                UserPoints = -userPoints, // Negative for redeemed user points
                TransactionType = TransactionCategory.Redeemed,
                TransactionSource = TransactionOrigin.Redemption,
                SourceId = redemptionId,
                Description = description,
                Timestamp = DateTime.UtcNow,
                BalanceAfter = balanceAfter
            };

            await _unitOfWork.UserPointsTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserPointsTransaction>> GetUserTransactionsAsync(Guid userId)
        {
            return await _unitOfWork.UserPointsTransactions.FindAsync(t => t.UserId == userId);
        }

        public async Task<IEnumerable<UserPointsTransaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("From date must be before to date");

            return await _unitOfWork.UserPointsTransactions.FindAsync(t => t.Timestamp >= from && t.Timestamp <= to);
        }
    }
}