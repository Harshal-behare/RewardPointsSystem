using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Products
{
    /// <summary>
    /// Represents a product available for redemption with business logic
    /// </summary>
    public class Product
    {
        private readonly List<ProductPricing> _pricingHistory;
        private readonly List<Redemption> _redemptions;

        public Guid Id { get; private set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
        public string Name { get; private set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; private set; }

        public Guid? CategoryId { get; private set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; private set; }

        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        [Required(ErrorMessage = "Created by user ID is required")]
        public Guid CreatedBy { get; private set; }

        // Navigation Properties - Encapsulated collections
        public virtual User? Creator { get; private set; }
        public virtual ProductCategory? ProductCategory { get; private set; }
        public virtual IReadOnlyCollection<ProductPricing> PricingHistory => _pricingHistory.AsReadOnly();
        public virtual InventoryItem? Inventory { get; private set; }
        public virtual IReadOnlyCollection<Redemption> Redemptions => _redemptions.AsReadOnly();

        // EF Core requires a parameterless constructor
        private Product()
        {
            _pricingHistory = new List<ProductPricing>();
            _redemptions = new List<Redemption>();
            Name = string.Empty;
        }

        private Product(
            string name,
            Guid createdBy,
            string? description = null,
            Guid? categoryId = null,
            string? imageUrl = null) : this()
        {
            Id = Guid.NewGuid();
            Name = ValidateName(name);
            Description = description;
            CategoryId = categoryId;
            ImageUrl = imageUrl;
            CreatedBy = createdBy;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new product
        /// </summary>
        public static Product Create(
            string name,
            Guid createdBy,
            string? description = null,
            Guid? categoryId = null,
            string? imageUrl = null)
        {
            if (createdBy == Guid.Empty)
                throw new ArgumentException("Created by user ID cannot be empty.", nameof(createdBy));

            return new Product(name, createdBy, description, categoryId, imageUrl);
        }

        /// <summary>
        /// Updates product details
        /// </summary>
        public void UpdateDetails(
            string name,
            string? description = null,
            Guid? categoryId = null,
            string? imageUrl = null)
        {
            Name = ValidateName(name);
            Description = description;
            CategoryId = categoryId;
            ImageUrl = imageUrl;
        }

        /// <summary>
        /// Activates the product
        /// </summary>
        public void Activate()
        {
            if (IsActive)
                throw new InvalidProductDataException($"Product '{Id}' is already active.");

            IsActive = true;
        }

        /// <summary>
        /// Deactivates the product
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidProductDataException($"Product '{Id}' is already inactive.");

            IsActive = false;
        }

        /// <summary>
        /// Gets the current active pricing
        /// </summary>
        public ProductPricing? GetCurrentPricing()
        {
            var now = DateTime.UtcNow;
            return _pricingHistory
                .Where(p => p.IsActive &&
                           p.EffectiveFrom <= now &&
                           (!p.EffectiveTo.HasValue || p.EffectiveTo.Value > now))
                .OrderByDescending(p => p.EffectiveFrom)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the current points cost
        /// </summary>
        public int GetCurrentPointsCost()
        {
            var pricing = GetCurrentPricing();
            if (pricing == null)
                throw new ProductPricingNotFoundException(Id);

            return pricing.PointsCost;
        }

        /// <summary>
        /// Checks if product has active pricing
        /// </summary>
        public bool HasActivePricing()
        {
            return GetCurrentPricing() != null;
        }

        /// <summary>
        /// Checks if product is available for redemption
        /// </summary>
        public bool IsAvailableForRedemption()
        {
            return IsActive && 
                   HasActivePricing() && 
                   Inventory != null && 
                   Inventory.QuantityAvailable > 0;
        }

        /// <summary>
        /// Adds pricing to history (internal use)
        /// </summary>
        internal void AddPricing(ProductPricing pricing)
        {
            if (pricing == null)
                throw new ArgumentNullException(nameof(pricing));

            _pricingHistory.Add(pricing);
        }

        /// <summary>
        /// Sets the inventory (internal use for EF navigation)
        /// </summary>
        internal void SetInventory(InventoryItem inventory)
        {
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        /// <summary>
        /// Updates the product category
        /// </summary>
        public void UpdateCategory(Guid? categoryId)
        {
            CategoryId = categoryId;
        }

        /// <summary>
        /// Sets the category (internal use for EF navigation)
        /// </summary>
        internal void SetCategory(ProductCategory category)
        {
            ProductCategory = category;
            CategoryId = category?.Id;
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidProductDataException("Product name is required.");

            if (name.Length < 2 || name.Length > 200)
                throw new InvalidProductDataException("Product name must be between 2 and 200 characters.");

            return name.Trim();
        }
    }
}
