using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Products;

namespace RewardPointsSystem.Application.Services.Products
{
    /// <summary>
    /// Service: PricingService
    /// Responsibility: Manage product points cost (pricing in points) only
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PricingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SetProductPointsCostAsync(Guid productId, int points, DateTime effectiveFrom)
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
                pricing.Deactivate(effectiveFrom.AddTicks(-1));
            }

            // Create new pricing
            var newPricing = ProductPricing.Create(productId, points, effectiveFrom);

            await _unitOfWork.Pricing.AddAsync(newPricing);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetCurrentPointsCostAsync(Guid productId)
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

        public async Task<IEnumerable<ProductPricing>> GetPointsCostHistoryAsync(Guid productId)
        {
            var allPricing = await _unitOfWork.Pricing.GetAllAsync();
            return allPricing
                .Where(p => p.ProductId == productId)
                .OrderByDescending(p => p.EffectiveFrom);
        }

        public async Task UpdatePointsCostAsync(Guid productId, int newPoints)
        {
            await SetProductPointsCostAsync(productId, newPoints, DateTime.UtcNow);
        }
    }
}