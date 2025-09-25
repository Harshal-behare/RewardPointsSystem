using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RewardPointsSystem.Models
{
    public class Product
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredPoints { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        
        public Product(string name, int requiredPoints, string category = "General", string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required", nameof(name));
            
            if (requiredPoints <= 0)
                throw new ArgumentException("Required points must be positive", nameof(requiredPoints));
            
            Name = name;
            RequiredPoints = requiredPoints;
            Category = category;
            Description = description;
        }
    }
}

