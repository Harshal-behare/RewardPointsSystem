using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Models.Products
{
    /// <summary>
    /// Represents pricing information for a product in points
    /// </summary>
    public class ProductPricing
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Points cost is required")]
        [Range(1, 1000000, ErrorMessage = "Points cost must be between 1 and 1,000,000")]
        public int PointsCost { get; set; }

        [Required(ErrorMessage = "Effective from date is required")]
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
