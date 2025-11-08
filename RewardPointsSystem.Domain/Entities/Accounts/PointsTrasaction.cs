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
    /// Represents a points transaction (earned or redeemed)
    /// </summary>
    public class PointsTransaction
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Points value is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points must be a positive value")]
        public int Points { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        public TransactionCategory TransactionType { get; set; }

        [Required(ErrorMessage = "Transaction source is required")]
        public TransactionOrigin TransactionSource { get; set; }

        [Required(ErrorMessage = "Source ID is required")]
        public Guid SourceId { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public DateTime Timestamp { get; set; }

        [Required(ErrorMessage = "Balance after transaction is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Balance after cannot be negative")]
        public int BalanceAfter { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }

        public PointsTransaction()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }
}
