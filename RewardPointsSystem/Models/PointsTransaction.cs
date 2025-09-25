using System;

namespace RewardPointsSystem.Models
{
    public class PointsTransaction
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public User User { get; private set; }
        public int Points { get; private set; }
        public string Type { get; private set; } // "Earn" or "Redeem"
        public string Description { get; private set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public int BalanceAfterTransaction { get; private set; }
        public Guid? RelatedEntityId { get; private set; } // Could be EventId, RedemptionId, etc.
        public string RelatedEntityType { get; private set; } // "Event", "Redemption", "Manual", etc.

        public PointsTransaction(User user, int points, string type, string description = "")
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Transaction type is required", nameof(type));

            User = user;
            Points = points;
            Type = type;
            Description = description;
            BalanceAfterTransaction = user.PointsBalance;
        }

        public void SetRelatedEntity(Guid entityId, string entityType)
        {
            RelatedEntityId = entityId;
            RelatedEntityType = entityType;
        }
    }
}

