using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Models.Accounts
{
    public enum TransactionType
    {
        Earned,
        Redeemed
    }

    public enum SourceType
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
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Source type is required")]
        public SourceType Source { get; set; }

        [Required(ErrorMessage = "Source ID is required")]
        public Guid SourceId { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public DateTime Timestamp { get; set; }

        public PointsTransaction()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }
}
