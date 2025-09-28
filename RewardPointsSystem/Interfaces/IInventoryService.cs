using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Products;

namespace RewardPointsSystem.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryItem> AddInventoryAsync(Guid productId, int quantity, string location);
        Task UpdateStockAsync(Guid productId, int quantity, string reason);
        Task<int> GetAvailableStockAsync(Guid productId);
        Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync(int threshold = 10);
        Task<bool> ReserveStockAsync(Guid productId, int quantity);
        Task ReleaseReservedStockAsync(Guid productId, int quantity);
    }
}