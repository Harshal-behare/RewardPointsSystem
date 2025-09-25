using System;
using System.Collections.Generic;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IInventoryService
    {
        void AddInventoryItem(Guid productId, int initialQuantity);
        void AddStock(Guid productId, int quantity);
        bool CheckAvailability(Guid productId, int quantity);
        void ReserveStock(Guid productId, int quantity);
        void ConfirmReservation(Guid productId, int quantity);
        void CancelReservation(Guid productId, int quantity);
        InventoryItem GetInventoryByProductId(Guid productId);
        IEnumerable<InventoryItem> GetAllInventoryItems();
        int GetAvailableQuantity(Guid productId);
    }
}