using System;

namespace RewardPointsSystem.Models
{
    public class Redemption
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public User User { get; set; }
        public Product Product { get; set; }
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public int PointsRedeemed { get; set; }
        
        // Alias for backward compatibility
        public DateTime Date => Timestamp;
    }
}

