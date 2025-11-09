using System;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Products
{
    /// <summary>
    /// Represents inventory information for a product with business logic
    /// </summary>
    public class InventoryItem
    {
        public Guid Id { get; private set; }

        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity available cannot be negative")]
        public int QuantityAvailable { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity reserved cannot be negative")]
        public int QuantityReserved { get; private set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
        public int ReorderLevel { get; private set; }

        public DateTime LastRestocked { get; private set; }
        public DateTime LastUpdated { get; private set; }
        public Guid? UpdatedBy { get; private set; }

        // Navigation Properties
        public virtual Product? Product { get; private set; }

        // EF Core requires a parameterless constructor
        private InventoryItem()
        {
        }

        private InventoryItem(
            Guid productId,
            int initialQuantity,
            int reorderLevel)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            QuantityAvailable = ValidateQuantity(initialQuantity, nameof(initialQuantity));
            QuantityReserved = 0;
            ReorderLevel = ValidateQuantity(reorderLevel, nameof(reorderLevel));
            LastRestocked = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new inventory item
        /// </summary>
        public static InventoryItem Create(
            Guid productId,
            int initialQuantity = 0,
            int reorderLevel = 0)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

            return new InventoryItem(productId, initialQuantity, reorderLevel);
        }

        /// <summary>
        /// Restocks inventory with additional quantity
        /// </summary>
        public void Restock(int quantity, Guid updatedBy)
        {
            ValidateQuantity(quantity, nameof(quantity));

            if (quantity == 0)
                throw new ArgumentException("Restock quantity must be greater than zero.", nameof(quantity));

            QuantityAvailable += quantity;
            LastRestocked = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Reserves quantity for a redemption
        /// </summary>
        public void Reserve(int quantity)
        {
            ValidateQuantity(quantity, nameof(quantity));

            if (quantity == 0)
                throw new ArgumentException("Reserve quantity must be greater than zero.", nameof(quantity));

            if (QuantityAvailable < quantity)
                throw new InsufficientInventoryException(ProductId, quantity, QuantityAvailable);

            QuantityAvailable -= quantity;
            QuantityReserved += quantity;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Releases reserved quantity (e.g., when redemption is cancelled)
        /// </summary>
        public void Release(int quantity)
        {
            ValidateQuantity(quantity, nameof(quantity));

            if (quantity == 0)
                throw new ArgumentException("Release quantity must be greater than zero.", nameof(quantity));

            if (QuantityReserved < quantity)
                throw new InvalidOperationException($"Cannot release {quantity} items. Only {QuantityReserved} reserved.");

            QuantityReserved -= quantity;
            QuantityAvailable += quantity;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Confirms fulfillment of reserved quantity
        /// </summary>
        public void ConfirmFulfillment(int quantity)
        {
            ValidateQuantity(quantity, nameof(quantity));

            if (quantity == 0)
                throw new ArgumentException("Fulfillment quantity must be greater than zero.", nameof(quantity));

            if (QuantityReserved < quantity)
                throw new InvalidOperationException($"Cannot fulfill {quantity} items. Only {QuantityReserved} reserved.");

            QuantityReserved -= quantity;
            LastUpdated = DateTime.UtcNow;
        }

        /// <summary>
        /// Adjusts inventory (for corrections, with reason)
        /// </summary>
        public void Adjust(int quantityChange, Guid updatedBy)
        {
            var newQuantity = QuantityAvailable + quantityChange;

            if (newQuantity < 0)
                throw new InvalidOperationException($"Adjustment would result in negative quantity: {newQuantity}");

            QuantityAvailable = newQuantity;
            LastUpdated = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Updates the reorder level
        /// </summary>
        public void UpdateReorderLevel(int reorderLevel, Guid updatedBy)
        {
            ReorderLevel = ValidateQuantity(reorderLevel, nameof(reorderLevel));
            LastUpdated = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// Checks if inventory is below reorder level
        /// </summary>
        public bool IsBelowReorderLevel() => QuantityAvailable <= ReorderLevel;

        /// <summary>
        /// Checks if inventory is available for reservation
        /// </summary>
        public bool CanReserve(int quantity) => QuantityAvailable >= quantity;

        /// <summary>
        /// Gets total inventory including reserved
        /// </summary>
        public int GetTotalInventory() => QuantityAvailable + QuantityReserved;

        private static int ValidateQuantity(int quantity, string paramName)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative.", paramName);

            return quantity;
        }
    }
}
