using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Products;

namespace RewardPointsSystem.Interfaces
{
    public interface IPricingService
    {
        Task<ProductPricing> SetPricingAsync(Guid productId, decimal standardPrice, decimal premiumPrice);
        Task<ProductPricing> UpdatePricingAsync(Guid productId, decimal standardPrice, decimal premiumPrice);
        Task<ProductPricing> GetPricingAsync(Guid productId);
        Task<IEnumerable<ProductPricing>> GetAllPricingsAsync();
        Task RemovePricingAsync(Guid productId);
        Task<decimal> CalculateUserPriceAsync(Guid productId, Guid userId);
    }
}