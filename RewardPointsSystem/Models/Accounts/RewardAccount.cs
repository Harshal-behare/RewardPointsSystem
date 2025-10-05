using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Models.Accounts
{
    /// <summary>
    /// Represents a user's reward account with points balance
    /// </summary>
    public class RewardAccount
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current balance cannot be negative")]
        public int CurrentBalance { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total earned cannot be negative")]
        public int TotalEarned { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Total redeemed cannot be negative")]
        public int TotalRedeemed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public RewardAccount()
        {
            Id = Guid.NewGuid();
            CurrentBalance = 0;
            TotalEarned = 0;
            TotalRedeemed = 0;
            CreatedAt = DateTime.UtcNow;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
}
