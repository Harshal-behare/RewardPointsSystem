using System;
using System.Collections.Generic;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly List<InventoryItem> _inventoryItems = new();
        private readonly object _lockObject = new(); // Thread safety for future scalability

        public void AddInventoryItem(Guid productId, int initialQuantity)
        {
            lock (_lockObject)
            {
                if (_inventoryItems.Any(i => i.ProductId == productId))
                    throw new InvalidOperationException($"Inventory item for product {productId} already exists");

                var inventoryItem = new InventoryItem(productId, initialQuantity);
                _inventoryItems.Add(inventoryItem);
            }
        }

        public void AddStock(Guid productId, int quantity)
        {
            lock (_lockObject)
            {
                var item = GetInventoryByProductId(productId);
                if (item == null)
                    throw new InvalidOperationException($"No inventory found for product {productId}");

                item.AddStock(quantity);
            }
        }

        public bool CheckAvailability(Guid productId, int quantity)
        {
            lock (_lockObject)
            {
                var item = GetInventoryByProductId(productId);
                return item != null && item.HasAvailableStock(quantity);
            }
        }

        public void ReserveStock(Guid productId, int quantity)
        {
            lock (_lockObject)
            {
                var item = GetInventoryByProductId(productId);
                if (item == null)
                    throw new InvalidOperationException($"No inventory found for product {productId}");

                item.ReserveStock(quantity);
            }
        }

        public void ConfirmReservation(Guid productId, int quantity)
        {
            lock (_lockObject)
            {
                var item = GetInventoryByProductId(productId);
                if (item == null)
                    throw new InvalidOperationException($"No inventory found for product {productId}");

                item.ConfirmReservation(quantity);
            }
        }

        public void CancelReservation(Guid productId, int quantity)
        {
            lock (_lockObject)
            {
                var item = GetInventoryByProductId(productId);
                if (item == null)
                    throw new InvalidOperationException($"No inventory found for product {productId}");

                item.CancelReservation(quantity);
            }
        }

        public InventoryItem GetInventoryByProductId(Guid productId)
        {
            lock (_lockObject)
            {
                return _inventoryItems.FirstOrDefault(i => i.ProductId == productId);
            }
        }

        public IEnumerable<InventoryItem> GetAllInventoryItems()
        {
            lock (_lockObject)
            {
                return _inventoryItems.ToList(); // Return a copy for thread safety
            }
        }

        public int GetAvailableQuantity(Guid productId)
        {
            lock (_lockObject)
            {
                var item = GetInventoryByProductId(productId);
                return item?.QuantityAvailable ?? 0;
            }
        }
    }
}