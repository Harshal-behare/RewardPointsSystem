using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Products;

namespace RewardPointsSystem.Interfaces
{
    /// <summary>
    /// Interface: IPricingService
    /// Responsibility: Manage product points cost (pricing in points) only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IPricingService
    {
        Task SetProductPointsCostAsync(Guid productId, int points, DateTime effectiveFrom);
        Task<int> GetCurrentPointsCostAsync(Guid productId);
        Task<IEnumerable<ProductPricing>> GetPointsCostHistoryAsync(Guid productId);
        Task UpdatePointsCostAsync(Guid productId, int newPoints);
    }
}
