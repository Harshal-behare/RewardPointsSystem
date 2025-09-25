using System;

namespace RewardPointsSystem.Models
{
    public class InventoryItem
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid ProductId { get; private set; }
        public int QuantityAvailable { get; private set; }
        public int QuantityReserved { get; private set; }
        public DateTime LastUpdated { get; private set; }
        
        public int TotalQuantity => QuantityAvailable + QuantityReserved;

        public InventoryItem(Guid productId, int initialQuantity)
        {
            if (initialQuantity < 0)
                throw new ArgumentException("Initial quantity cannot be negative", nameof(initialQuantity));
            
            ProductId = productId;
            QuantityAvailable = initialQuantity;
            QuantityReserved = 0;
            LastUpdated = DateTime.UtcNow;
        }

        public void AddStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
            QuantityAvailable += quantity;
            LastUpdated = DateTime.UtcNow;
        }

        public void ReserveStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
            if (quantity > QuantityAvailable)
                throw new InvalidOperationException($"Cannot reserve {quantity} items. Only {QuantityAvailable} available.");
            
            QuantityAvailable -= quantity;
            QuantityReserved += quantity;
            LastUpdated = DateTime.UtcNow;
        }

        public void ConfirmReservation(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
            if (quantity > QuantityReserved)
                throw new InvalidOperationException($"Cannot confirm {quantity} items. Only {QuantityReserved} reserved.");
            
            QuantityReserved -= quantity;
            LastUpdated = DateTime.UtcNow;
        }

        public void CancelReservation(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));
            
            if (quantity > QuantityReserved)
                throw new InvalidOperationException($"Cannot cancel {quantity} items. Only {QuantityReserved} reserved.");
            
            QuantityReserved -= quantity;
            QuantityAvailable += quantity;
            LastUpdated = DateTime.UtcNow;
        }

        public bool HasAvailableStock(int quantity)
        {
            return quantity > 0 && quantity <= QuantityAvailable;
        }
    }
}