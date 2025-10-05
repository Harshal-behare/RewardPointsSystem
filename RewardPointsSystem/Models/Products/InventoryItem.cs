using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Models.Products
{
    /// <summary>
    /// Represents inventory information for a product
    /// </summary>
    public class InventoryItem
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity available cannot be negative")]
        public int QuantityAvailable { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity reserved cannot be negative")]
        public int QuantityReserved { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
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
