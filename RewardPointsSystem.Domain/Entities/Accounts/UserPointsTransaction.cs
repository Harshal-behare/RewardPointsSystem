using System;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Domain.Entities.Accounts
{
    public enum TransactionCategory
    {
        Earned,
        Redeemed
    }

    public enum TransactionOrigin
    {
        Event,
        Redemption
    }

    /// <summary>
    /// Represents a points transaction (earned or redeemed) - Immutable record
    /// </summary>
    public class UserPointsTransaction
    {
        public Guid Id { get; private set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; private set; }

        [Required(ErrorMessage = "Points value is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points must be a positive value")]
        public int UserPoints { get; private set; }

        [Required(ErrorMessage = "Transaction type is required")]
        public TransactionCategory TransactionType { get; private set; }

        [Required(ErrorMessage = "Transaction source is required")]
        public TransactionOrigin TransactionSource { get; private set; }

        [Required(ErrorMessage = "Source ID is required")]
        public Guid SourceId { get; private set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; private set; }

        public DateTime Timestamp { get; private set; }

        [Required(ErrorMessage = "Balance after transaction is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Balance after cannot be negative")]
        public int BalanceAfter { get; private set; }

        // Navigation Properties
        public virtual User? User { get; private set; }

        // EF Core requires a parameterless constructor
        private UserPointsTransaction()
        {
        }

        private UserPointsTransaction(
            Guid userId,
            int userPoints,
            TransactionCategory transactionType,
            TransactionOrigin transactionSource,
            Guid sourceId,
            int balanceAfter,
            string? description = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            UserPoints = ValidatePoints(userPoints);
            TransactionType = transactionType;
            TransactionSource = transactionSource;
            SourceId = sourceId;
            BalanceAfter = ValidateBalance(balanceAfter);
            Description = description;
            Timestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a transaction for earned points
        /// </summary>
        public static UserPointsTransaction CreateEarned(
            Guid userId,
            int points,
            TransactionOrigin source,
            Guid sourceId,
            int balanceAfter,
            string? description = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (sourceId == Guid.Empty)
                throw new ArgumentException("Source ID cannot be empty.", nameof(sourceId));

            return new UserPointsTransaction(
                userId,
                points,
                TransactionCategory.Earned,
                source,
                sourceId,
                balanceAfter,
                description);
        }

        /// <summary>
        /// Factory method to create a transaction for redeemed points
        /// </summary>
        public static UserPointsTransaction CreateRedeemed(
            Guid userId,
            int points,
            Guid redemptionId,
            int balanceAfter,
            string? description = null)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (redemptionId == Guid.Empty)
                throw new ArgumentException("Redemption ID cannot be empty.", nameof(redemptionId));

            return new UserPointsTransaction(
                userId,
                points,
                TransactionCategory.Redeemed,
                TransactionOrigin.Redemption,
                redemptionId,
                balanceAfter,
                description);
        }

        /// <summary>
        /// Checks if transaction is for earned points
        /// </summary>
        public bool IsEarned() => TransactionType == TransactionCategory.Earned;

        /// <summary>
        /// Checks if transaction is for redeemed points
        /// </summary>
        public bool IsRedeemed() => TransactionType == TransactionCategory.Redeemed;

        private static int ValidatePoints(int points)
        {
            if (points < 1)
                throw new ArgumentException("Points must be a positive value.", nameof(points));

            return points;
        }

        private static int ValidateBalance(int balance)
        {
            if (balance < 0)
                throw new ArgumentException("Balance after cannot be negative.", nameof(balance));

            return balance;
        }
    }
}
