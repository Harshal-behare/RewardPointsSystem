using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Domain.Entities.Products
{
    /// <summary>
    /// Represents pricing information for a product in points
    /// </summary>
    public class ProductPricing
    {
        public Guid Id { get; private set; }

        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; private set; }

        [Required(ErrorMessage = "Points cost is required")]
        [Range(1, 1000000, ErrorMessage = "Points cost must be between 1 and 1,000,000")]
        public int PointsCost { get; private set; }

        [Required(ErrorMessage = "Effective from date is required")]
        public DateTime EffectiveFrom { get; private set; }

        public DateTime? EffectiveTo { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation Properties
        public virtual Product? Product { get; private set; }

        // EF Core requires a parameterless constructor
        private ProductPricing()
        {
        }

        private ProductPricing(
            Guid productId,
            int pointsCost,
            DateTime effectiveFrom)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            PointsCost = ValidatePointsCost(pointsCost);
            EffectiveFrom = effectiveFrom;
            IsActive = true;
        }

        /// <summary>
        /// Factory method to create a new pricing entry
        /// </summary>
        public static ProductPricing Create(
            Guid productId,
            int pointsCost,
            DateTime? effectiveFrom = null)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

            return new ProductPricing(
                productId,
                pointsCost,
                effectiveFrom ?? DateTime.UtcNow);
        }

        /// <summary>
        /// Deactivates the pricing
        /// </summary>
        public void Deactivate(DateTime? effectiveTo = null)
        {
            if (!IsActive)
                throw new InvalidOperationException("Pricing is already inactive.");

            IsActive = false;
            EffectiveTo = effectiveTo ?? DateTime.UtcNow;
        }

        /// <summary>
        /// Reactivates the pricing
        /// </summary>
        public void Reactivate()
        {
            if (IsActive)
                throw new InvalidOperationException("Pricing is already active.");

            IsActive = true;
            EffectiveTo = null;
        }

        /// <summary>
        /// Updates the points cost for this pricing
        /// </summary>
        public void UpdatePointsCost(int newPointsCost)
        {
            PointsCost = ValidatePointsCost(newPointsCost);
        }

        /// <summary>
        /// Checks if pricing is currently effective
        /// </summary>
        public bool IsCurrentlyEffective()
        {
            var now = DateTime.UtcNow;
            return IsActive &&
                   EffectiveFrom <= now &&
                   (!EffectiveTo.HasValue || EffectiveTo.Value > now);
        }

        private static int ValidatePointsCost(int pointsCost)
        {
            if (pointsCost < 1)
                throw new ArgumentException("Points cost must be at least 1.", nameof(pointsCost));

            if (pointsCost > 1000000)
                throw new ArgumentException("Points cost cannot exceed 1,000,000.", nameof(pointsCost));

            return pointsCost;
        }
    }
}
