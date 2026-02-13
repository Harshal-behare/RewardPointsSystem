using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Services.Products
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

            var inventory = InventoryItem.Create(productId, quantity, reorderLevel);

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

            inventory.Restock(quantity, null);

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

            inventory.Reserve(quantity);

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

            inventory.Release(quantity);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ConfirmFulfillmentAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
                throw new ArgumentException($"No inventory found for product {productId}", nameof(productId));

            inventory.ConfirmFulfillment(quantity);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<InventoryAlert>> GetLowStockItemsAsync()
        {
            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var allProducts = await _unitOfWork.Products.GetAllAsync();

            // Only show low stock alerts for active products
            var activeProducts = allProducts.Where(p => p.IsActive).ToList();
            var activeProductIds = activeProducts.Select(p => p.Id).ToHashSet();

            var lowStockItems = allInventory
                .Where(i => i.QuantityAvailable <= i.ReorderLevel && activeProductIds.Contains(i.ProductId))
                .Select(i => new InventoryAlert
                {
                    ProductId = i.ProductId,
                    ProductName = activeProducts.FirstOrDefault(p => p.Id == i.ProductId)?.Name ?? "Unknown",
                    CurrentStock = i.QuantityAvailable,
                    ReorderLevel = i.ReorderLevel,
                    AlertType = i.QuantityAvailable == 0 ? "OUT_OF_STOCK" : "LOW_STOCK"
                });

            return lowStockItems;
        }

        public async Task<int> GetAvailableStockAsync(Guid productId)
        {
            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
                return 0;

            // QuantityAvailable already accounts for reserved items (Reserve method subtracts from Available)
            return inventory.QuantityAvailable;
        }

        public async Task AdjustStockAsync(Guid productId, int targetQuantity, Guid adjustedBy)
        {
            var allInventory = await _unitOfWork.Inventory.GetAllAsync();
            var inventory = allInventory.FirstOrDefault(i => i.ProductId == productId);

            if (inventory == null)
            {
                // Create new inventory if it doesn't exist
                await CreateInventoryAsync(productId, targetQuantity, 10);
                return;
            }

            // The target is the displayed stock (QuantityAvailable - QuantityReserved)
            // So the target QuantityAvailable should be: targetQuantity + QuantityReserved
            var targetQuantityAvailable = targetQuantity + inventory.QuantityReserved;
            var currentStock = inventory.QuantityAvailable;
            var difference = targetQuantityAvailable - currentStock;

            if (difference != 0)
            {
                inventory.Adjust(difference, adjustedBy);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}