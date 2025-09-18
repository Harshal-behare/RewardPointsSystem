using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardPointsSystem.Models
{
    public class Redemption
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public User User { get; set; }
        public Product Product { get; set; }
        public DateTime Date { get; private set; } = DateTime.Now;
    }
}

