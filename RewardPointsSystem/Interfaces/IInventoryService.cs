using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Products;
using RewardPointsSystem.DTOs;

namespace RewardPointsSystem.Interfaces
{
    /// <summary>
    /// Interface: IInventoryService
    /// Responsibility: Manage product stock only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IInventoryService
    {
        Task<InventoryItem> CreateInventoryAsync(Guid productId, int quantity, int reorderLevel);
        Task AddStockAsync(Guid productId, int quantity);
        Task<bool> IsInStockAsync(Guid productId);
        Task ReserveStockAsync(Guid productId, int quantity);
        Task ReleaseReservationAsync(Guid productId, int quantity);
        Task<IEnumerable<InventoryAlert>> GetLowStockItemsAsync();
    }
}