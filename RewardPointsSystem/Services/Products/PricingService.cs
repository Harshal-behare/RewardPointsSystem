using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Products;

namespace RewardPointsSystem.Services.Products
{
    /// <summary>
    /// Service: PricingService
    /// Responsibility: Manage product pricing only
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PricingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SetProductPriceAsync(Guid productId, int points, DateTime effectiveFrom)
        {
            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                throw new ArgumentException($"Product with ID {productId} not found", nameof(productId));

            if (points <= 0)
                throw new ArgumentException("Points cost must be positive", nameof(points));

            // Deactivate current active pricing
            var allPricing = await _unitOfWork.Pricing.GetAllAsync();
            var currentPricing = allPricing.Where(p => p.ProductId == productId && p.IsActive);
            
            foreach (var pricing in currentPricing)
            {
                pricing.IsActive = false;
                pricing.EffectiveTo = effectiveFrom.AddTicks(-1); // End just before new pricing starts
            }

            // Create new pricing
            var newPricing = new ProductPricing
            {
                ProductId = productId,
                PointsCost = points,
                EffectiveFrom = effectiveFrom,
                IsActive = true
            };

            await _unitOfWork.Pricing.AddAsync(newPricing);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetCurrentPriceAsync(Guid productId)
        {
            var allPricing = await _unitOfWork.Pricing.GetAllAsync();
            var currentPricing = allPricing
                .Where(p => p.ProductId == productId && 
                           p.IsActive && 
                           p.EffectiveFrom <= DateTime.UtcNow &&
                           (p.EffectiveTo == null || p.EffectiveTo > DateTime.UtcNow))
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefault();

            if (currentPricing == null)
                throw new InvalidOperationException($"No active pricing found for product {productId}");

            return currentPricing.PointsCost;
        }

        public async Task<IEnumerable<ProductPricing>> GetPriceHistoryAsync(Guid productId)
        {
            var allPricing = await _unitOfWork.Pricing.GetAllAsync();
            return allPricing
                .Where(p => p.ProductId == productId)
                .OrderByDescending(p => p.EffectiveFrom);
        }

        public async Task UpdatePriceAsync(Guid productId, int newPoints)
        {
            await SetProductPriceAsync(productId, newPoints, DateTime.UtcNow);
        }
    }
}