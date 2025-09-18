using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardPointsSystem.Models
{
    public class Event
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Title { get; set; }
        public DateTime Date { get; set; }
    }
}

