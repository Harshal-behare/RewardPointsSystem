using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Domain.Entities.Products
{
    /// <summary>
    /// Represents a product category for classification and organization
    /// </summary>
    public class ProductCategory
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Display order is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Display order cannot be negative")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; }

        public ProductCategory()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            Products = new HashSet<Product>();
        }
    }
}
