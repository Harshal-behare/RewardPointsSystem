using System;

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

    public class PointsTransaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public TransactionType Type { get; set; }
        public SourceType Source { get; set; }
        public Guid SourceId { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }

        public PointsTransaction()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }
}