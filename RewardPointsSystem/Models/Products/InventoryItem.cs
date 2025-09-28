using System;

namespace RewardPointsSystem.Models.Products
{
    public class InventoryItem
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime LastRestocked { get; set; }
        public DateTime LastUpdated { get; set; }

        public InventoryItem()
        {
            Id = Guid.NewGuid();
            QuantityAvailable = 0;
            QuantityReserved = 0;
            LastRestocked = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }
    }
}