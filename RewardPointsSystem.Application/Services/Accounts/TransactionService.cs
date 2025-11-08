using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Services.Accounts
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task RecordEarnedPointsAsync(Guid userId, int points, Guid eventId, string description)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            // Get current balance to calculate balance after
            var account = await _unitOfWork.PointsAccounts.SingleOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                throw new InvalidOperationException($"Points account not found for user {userId}");

            var balanceAfter = account.CurrentBalance + points;

            var transaction = new PointsTransaction
            {
                UserId = userId,
                Points = points,
                TransactionType = TransactionCategory.Earned,
                TransactionSource = TransactionOrigin.Event,
                SourceId = eventId,
                Description = description,
                Timestamp = DateTime.UtcNow,
                BalanceAfter = balanceAfter
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RecordRedeemedPointsAsync(Guid userId, int points, Guid redemptionId, string description)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            // Get current balance to calculate balance after
            var account = await _unitOfWork.PointsAccounts.SingleOrDefaultAsync(a => a.UserId == userId);
            if (account == null)
                throw new InvalidOperationException($"Points account not found for user {userId}");

            var balanceAfter = account.CurrentBalance - points;

            var transaction = new PointsTransaction
            {
                UserId = userId,
                Points = -points, // Negative for redeemed points
                TransactionType = TransactionCategory.Redeemed,
                TransactionSource = TransactionOrigin.Redemption,
                SourceId = redemptionId,
                Description = description,
                Timestamp = DateTime.UtcNow,
                BalanceAfter = balanceAfter
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<PointsTransaction>> GetUserTransactionsAsync(Guid userId)
        {
            return await _unitOfWork.Transactions.FindAsync(t => t.UserId == userId);
        }

        public async Task<IEnumerable<PointsTransaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("From date must be before to date");

            return await _unitOfWork.Transactions.FindAsync(t => t.Timestamp >= from && t.Timestamp <= to);
        }
    }
}