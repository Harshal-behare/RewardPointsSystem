using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Products;

namespace RewardPointsSystem.Interfaces
{
    /// <summary>
    /// Interface: IPricingService
    /// Responsibility: Manage product pricing only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IPricingService
    {
        Task SetProductPriceAsync(Guid productId, int points, DateTime effectiveFrom);
        Task<int> GetCurrentPriceAsync(Guid productId);
        Task<IEnumerable<ProductPricing>> GetPriceHistoryAsync(Guid productId);
        Task UpdatePriceAsync(Guid productId, int newPoints);
    }
}