using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Services.Accounts
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<PointsTransaction> RecordEarnedPointsAsync(Guid userId, decimal points, Guid sourceId, string description)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            var transaction = new PointsTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Points = points, // Positive for earned points
                TransactionType = TransactionType.Earned,
                SourceType = SourceType.Event,
                SourceId = sourceId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PointsTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        public async Task<PointsTransaction> RecordRedeemedPointsAsync(Guid userId, decimal points, Guid sourceId, string description)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required", nameof(description));

            // Validate user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found");

            var transaction = new PointsTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Points = -points, // Negative for redeemed points
                TransactionType = TransactionType.Redeemed,
                SourceType = SourceType.Redemption,
                SourceId = sourceId,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PointsTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return transaction;
        }

        public async Task<IEnumerable<PointsTransaction>> GetUserTransactionsAsync(Guid userId)
        {
            return await _unitOfWork.PointsTransactions.FindAsync(t => t.UserId == userId);
        }

        public async Task<IEnumerable<PointsTransaction>> GetUserTransactionsByTypeAsync(Guid userId, TransactionType transactionType)
        {
            return await _unitOfWork.PointsTransactions.FindAsync(t => t.UserId == userId && t.TransactionType == transactionType);
        }

        public async Task<IEnumerable<PointsTransaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to)
        {
            if (from > to)
                throw new ArgumentException("From date must be before or equal to To date");

            return await _unitOfWork.PointsTransactions.FindAsync(t => t.CreatedAt >= from && t.CreatedAt <= to);
        }

        public async Task<IEnumerable<PointsTransaction>> GetTransactionsBySourceAsync(Guid sourceId, SourceType sourceType)
        {
            return await _unitOfWork.PointsTransactions.FindAsync(t => t.SourceId == sourceId && t.SourceType == sourceType);
        }

        public async Task<decimal> GetUserTotalEarnedAsync(Guid userId)
        {
            var earnedTransactions = await _unitOfWork.PointsTransactions.FindAsync(
                t => t.UserId == userId && t.TransactionType == TransactionType.Earned);

            decimal total = 0;
            foreach (var transaction in earnedTransactions)
            {
                total += transaction.Points;
            }

            return total;
        }

        public async Task<decimal> GetUserTotalRedeemedAsync(Guid userId)
        {
            var redeemedTransactions = await _unitOfWork.PointsTransactions.FindAsync(
                t => t.UserId == userId && t.TransactionType == TransactionType.Redeemed);

            decimal total = 0;
            foreach (var transaction in redeemedTransactions)
            {
                total += Math.Abs(transaction.Points); // Convert negative values to positive for total
            }

            return total;
        }

        public async Task<decimal> GetUserCurrentBalanceAsync(Guid userId)
        {
            var allTransactions = await _unitOfWork.PointsTransactions.FindAsync(t => t.UserId == userId);

            decimal balance = 0;
            foreach (var transaction in allTransactions)
            {
                balance += transaction.Points; // Earned are positive, redeemed are negative
            }

            return balance;
        }

        public async Task<TransactionSummaryDto> GetUserTransactionSummaryAsync(Guid userId, DateTime? from = null, DateTime? to = null)
        {
            var transactions = await _unitOfWork.PointsTransactions.FindAsync(t => 
                t.UserId == userId &&
                (!from.HasValue || t.CreatedAt >= from.Value) &&
                (!to.HasValue || t.CreatedAt <= to.Value));

            decimal totalEarned = 0;
            decimal totalRedeemed = 0;
            decimal currentBalance = 0;
            int earnedCount = 0;
            int redeemedCount = 0;

            foreach (var transaction in transactions)
            {
                currentBalance += transaction.Points;

                if (transaction.TransactionType == TransactionType.Earned)
                {
                    totalEarned += transaction.Points;
                    earnedCount++;
                }
                else
                {
                    totalRedeemed += Math.Abs(transaction.Points);
                    redeemedCount++;
                }
            }

            return new TransactionSummaryDto
            {
                UserId = userId,
                TotalEarned = totalEarned,
                TotalRedeemed = totalRedeemed,
                CurrentBalance = currentBalance,
                EarnedTransactionCount = earnedCount,
                RedeemedTransactionCount = redeemedCount,
                From = from,
                To = to,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}