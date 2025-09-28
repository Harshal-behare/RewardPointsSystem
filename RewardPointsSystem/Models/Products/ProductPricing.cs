using System;

namespace RewardPointsSystem.Models.Products
{
    public class ProductPricing
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public int PointsCost { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }

        public ProductPricing()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            EffectiveFrom = DateTime.UtcNow;
        }
    }
}