using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Products;
using RewardPointsSystem.DTOs;

namespace RewardPointsSystem.Services.Products
{
    /// <summary>
    /// Service: InventoryService
    /// Responsibility: Manage product stock only
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<InventoryItem> CreateInventoryAsync(Guid productId, int quantity, int reorderLevel)
        {
            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found", nameof(productId));

            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            if (reorderLevel <= 0)
                throw new ArgumentException("Reorder level must be positive", nameof(reorderLevel));

            // Check if inventory already exists for this product
            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var existingInventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (existingInventory != null)
                throw new InvalidOperationException($"Inventory already exists for product {productId}");

            var inventory = new InventoryItem
            {
                ProductId = productId,
                QuantityAvailable = quantity,
                QuantityReserved = 0,
                ReorderLevel = reorderLevel,
                LastRestocked = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _unitOfWork.Inventory.AddAsync(inventory);
            await _unitOfWork.SaveChangesAsync();

            return inventory;
        }

        public async Task AddStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
                throw new ArgumentException($"No inventory found for product {productId}", nameof(productId));

            inventory.QuantityAvailable += quantity;
            inventory.LastRestocked = DateTime.UtcNow;
            inventory.LastUpdated = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> IsInStockAsync(Guid productId)
        {
            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
                return false;

            return inventory.QuantityAvailable > 0;
        }

        public async Task ReserveStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
                throw new ArgumentException($"No inventory found for product {productId}", nameof(productId));

            if (inventory.QuantityAvailable < quantity)
                throw new InvalidOperationException($"Insufficient stock. Available: {inventory.QuantityAvailable}, Requested: {quantity}");

            inventory.QuantityAvailable -= quantity;
            inventory.QuantityReserved += quantity;
            inventory.LastUpdated = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ReleaseReservationAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
                throw new ArgumentException($"No inventory found for product {productId}", nameof(productId));

            if (inventory.QuantityReserved < quantity)
                throw new InvalidOperationException($"Cannot release more than reserved. Reserved: {inventory.QuantityReserved}, Requested: {quantity}");

            inventory.QuantityAvailable += quantity;
            inventory.QuantityReserved -= quantity;
            inventory.LastUpdated = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<InventoryAlert>> GetLowStockItemsAsync()
        {
            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var allProducts = await _unitOfWork.Products.GetAllAsync();

            var lowStockItems = allInventory
                .Where(i => i.QuantityAvailable <= i.ReorderLevel)
                .Select(i => new InventoryAlert
                {
                    ProductId = i.ProductId,
                    ProductName = allProducts.FirstOrDefault(p => p.Id == i.ProductId)?.Name ?? "Unknown",
                    CurrentStock = i.QuantityAvailable,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = i.QuantityAvailable == 0 ? "OUT_OF_STOCK" : "LOW_STOCK"
                });

            return lowStockItems;
        }
    }
}