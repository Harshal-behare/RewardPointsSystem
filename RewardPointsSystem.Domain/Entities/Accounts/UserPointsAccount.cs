using System;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Accounts
{
    /// <summary>
    /// Represents a user's points account with points balance
    /// Rich domain model with business logic for point operations
    /// </summary>
    public class UserPointsAccount
    {
        public Guid Id { get; private set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current balance cannot be negative")]
        public int CurrentBalance { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total earned cannot be negative")]
        public int TotalEarned { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total redeemed cannot be negative")]
        public int TotalRedeemed { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        // Navigation Properties
        public virtual User? User { get; private set; }

        // EF Core requires a parameterless constructor
        private UserPointsAccount()
        {
        }

        private UserPointsAccount(Guid userId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            CurrentBalance = 0;
            TotalEarned = 0;
            TotalRedeemed = 0;
            CreatedAt = DateTime.UtcNow;
            LastUpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new points account for a user
        /// </summary>
        public static UserPointsAccount CreateForUser(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            return new UserPointsAccount(userId);
        }

        /// <summary>
        /// Credits points to the account (for earning points)
        /// </summary>
        public void CreditPoints(int points, Guid creditedBy)
        {
            ValidatePointsAmount(points);

            CurrentBalance += points;
            TotalEarned += points;
            LastUpdatedAt = DateTime.UtcNow;
            UpdatedBy = creditedBy;
        }

        /// <summary>
        /// Debits points from the account (for redemption)
        /// </summary>
        public void DebitPoints(int points, Guid debitedBy)
        {
            ValidatePointsAmount(points);

            if (CurrentBalance < points)
            {
                throw new InsufficientUserPointsBalanceException(
                    UserId, 
                    points, 
                    CurrentBalance);
            }

            CurrentBalance -= points;
            TotalRedeemed += points;
            LastUpdatedAt = DateTime.UtcNow;
            UpdatedBy = debitedBy;
        }

        /// <summary>
        /// Checks if account has sufficient balance
        /// </summary>
        public bool HasSufficientBalance(int requiredPoints)
        {
            ValidatePointsAmount(requiredPoints);
            return CurrentBalance >= requiredPoints;
        }

        /// <summary>
        /// Reverses a credit operation (used for corrections)
        /// </summary>
        public void ReverseCreditPoints(int points, Guid reversedBy)
        {
            ValidatePointsAmount(points);

            if (CurrentBalance < points)
            {
                throw new InvalidUserPointsOperationException(
                    $"Cannot reverse credit of {points} points. Current balance is {CurrentBalance}.");
            }

            if (TotalEarned < points)
            {
                throw new InvalidUserPointsOperationException(
                    $"Cannot reverse credit of {points} points. Total earned is {TotalEarned}.");
            }

            CurrentBalance -= points;
            TotalEarned -= points;
            LastUpdatedAt = DateTime.UtcNow;
            UpdatedBy = reversedBy;
        }

        /// <summary>
        /// Reverses a debit operation (used for refunds)
        /// </summary>
        public void ReverseDebitPoints(int points, Guid reversedBy)
        {
            ValidatePointsAmount(points);

            if (TotalRedeemed < points)
            {
                throw new InvalidUserPointsOperationException(
                    $"Cannot reverse debit of {points} points. Total redeemed is {TotalRedeemed}.");
            }

            CurrentBalance += points;
            TotalRedeemed -= points;
            LastUpdatedAt = DateTime.UtcNow;
            UpdatedBy = reversedBy;
        }

        private static void ValidatePointsAmount(int points)
        {
            if (points <= 0)
                throw new ArgumentException("Points amount must be greater than zero.", nameof(points));

            if (points > int.MaxValue / 2) // Safety check against overflow
                throw new ArgumentException("Points amount is too large.", nameof(points));
        }
    }
}
