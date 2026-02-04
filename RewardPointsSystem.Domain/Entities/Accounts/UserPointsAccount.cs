using System;
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

        public Guid UserId { get; private set; }

        public int CurrentBalance { get; private set; }

        public int TotalEarned { get; private set; }

        public int TotalRedeemed { get; private set; }

        public int PendingPoints { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        public virtual User? User { get; private set; }

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
            PendingPoints = 0;
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
        public void CreditPoints(int points, Guid? creditedBy = null)
        {
            ValidatePointsAmount(points);

            CurrentBalance += points;
            TotalEarned += points;
            LastUpdatedAt = DateTime.UtcNow;
            
            // Only set UpdatedBy if a valid user ID is provided
            if (creditedBy.HasValue && creditedBy.Value != Guid.Empty)
            {
                UpdatedBy = creditedBy.Value;
            }
        }

        /// <summary>
        /// Debits points from the account (for redemption)
        /// </summary>
        public void DebitPoints(int points, Guid? debitedBy = null)
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
            
            // Only set UpdatedBy if a valid user ID is provided
            if (debitedBy.HasValue && debitedBy.Value != Guid.Empty)
            {
                UpdatedBy = debitedBy.Value;
            }
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
        public void ReverseCreditPoints(int points, Guid? reversedBy = null)
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
            
            if (reversedBy.HasValue && reversedBy.Value != Guid.Empty)
            {
                UpdatedBy = reversedBy.Value;
            }
        }

        /// <summary>
        /// Reverses a debit operation (used for refunds)
        /// </summary>
        public void ReverseDebitPoints(int points, Guid? reversedBy = null)
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
            
            if (reversedBy.HasValue && reversedBy.Value != Guid.Empty)
            {
                UpdatedBy = reversedBy.Value;
            }
        }

        /// <summary>
        /// Adds pending points (when a redemption is created)
        /// </summary>
        public void AddPendingPoints(int points, Guid? updatedBy = null)
        {
            ValidatePointsAmount(points);

            PendingPoints += points;
            LastUpdatedAt = DateTime.UtcNow;
            
            if (updatedBy.HasValue && updatedBy.Value != Guid.Empty)
            {
                UpdatedBy = updatedBy.Value;
            }
        }

        /// <summary>
        /// Releases pending points (when redemption is approved/delivered/completed)
        /// </summary>
        public void ReleasePendingPoints(int points, Guid? updatedBy = null)
        {
            ValidatePointsAmount(points);

            if (PendingPoints < points)
            {
                // Just clear what we have if trying to release more than pending
                PendingPoints = 0;
            }
            else
            {
                PendingPoints -= points;
            }
            
            LastUpdatedAt = DateTime.UtcNow;
            
            if (updatedBy.HasValue && updatedBy.Value != Guid.Empty)
            {
                UpdatedBy = updatedBy.Value;
            }
        }

        /// <summary>
        /// Cancels pending points and returns them to balance (when redemption is cancelled/rejected)
        /// </summary>
        public void CancelPendingPoints(int points, Guid? updatedBy = null)
        {
            ValidatePointsAmount(points);

            if (PendingPoints < points)
            {
                // Release what we have and add back to balance
                CurrentBalance += PendingPoints;
                TotalRedeemed -= PendingPoints;
                PendingPoints = 0;
            }
            else
            {
                PendingPoints -= points;
                // Note: CancelPendingPoints also reverses the debit
                CurrentBalance += points;
                TotalRedeemed -= points;
            }
            
            LastUpdatedAt = DateTime.UtcNow;
            
            if (updatedBy.HasValue && updatedBy.Value != Guid.Empty)
            {
                UpdatedBy = updatedBy.Value;
            }
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
