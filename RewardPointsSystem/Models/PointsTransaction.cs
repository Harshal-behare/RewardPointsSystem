using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardPointsSystem.Models
{
    public class PointsTransaction
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public User User { get; set; }
        public int Points { get; set; }
        public string Type { get; set; } // "Earn" or "Redeem"
        public DateTime Timestamp { get; private set; } = DateTime.Now;
    }
}

