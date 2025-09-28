using System;

namespace RewardPointsSystem.Models.Accounts
{
    public class RewardAccount
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int CurrentBalance { get; set; }
        public int TotalEarned { get; set; }
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