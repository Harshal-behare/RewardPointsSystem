using System;
using System.ComponentModel.DataAnnotations;

namespace RewardPointsSystem.Models.Products
{
    /// <summary>
    /// Represents a product available for redemption
    /// </summary>
    public class Product
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
        public string Category { get; set; } // "Electronics", "Gift Cards",  "Merchandise", "OfficeSupplies","Other"

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string ImageUrl { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "Created by user ID is required")]
        public Guid CreatedBy { get; set; }

        public Product()
        {
            Id = Guid.NewGuid();
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
