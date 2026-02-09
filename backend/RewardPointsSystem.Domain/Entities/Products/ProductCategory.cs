using System;
using System.Collections.Generic;
using System.Linq;

namespace RewardPointsSystem.Domain.Entities.Products
{
    /// <summary>
    /// Represents a product category for classification and organization
    /// </summary>
    public class ProductCategory
    {
        private readonly List<Product> _products;

        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string? Description { get; private set; }

        public int DisplayOrder { get; private set; }

        public bool IsActive { get; private set; }

        // Navigation Properties - Encapsulated collection
        public virtual IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        // EF Core requires a parameterless constructor
        private ProductCategory()
        {
            _products = new List<Product>();
            Name = string.Empty;
        }

        private ProductCategory(
            string name,
            int displayOrder,
            string? description = null) : this()
        {
            Id = Guid.NewGuid();
            Name = ValidateName(name);
            DisplayOrder = ValidateDisplayOrder(displayOrder);
            Description = description;
            IsActive = true;
        }

        /// <summary>
        /// Factory method to create a new product category
        /// </summary>
        public static ProductCategory Create(
            string name,
            int displayOrder,
            string? description = null)
        {
            return new ProductCategory(name, displayOrder, description);
        }

        /// <summary>
        /// Updates category information
        /// </summary>
        public void UpdateInfo(
            string name,
            int displayOrder,
            string? description = null)
        {
            Name = ValidateName(name);
            DisplayOrder = ValidateDisplayOrder(displayOrder);
            Description = description;
        }

        /// <summary>
        /// Activates the category
        /// </summary>
        public void Activate()
        {
            if (IsActive)
                throw new InvalidOperationException($"Category '{Id}' is already active.");

            IsActive = true;
        }

        /// <summary>
        /// Deactivates the category
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new InvalidOperationException($"Category '{Id}' is already inactive.");

            IsActive = false;
        }

        /// <summary>
        /// Gets active product count in this category
        /// </summary>
        public int GetActiveProductCount()
        {
            return _products.Count(p => p.IsActive);
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name is required.", nameof(name));

            if (name.Length < 2 || name.Length > 100)
                throw new ArgumentException("Category name must be between 2 and 100 characters.", nameof(name));

            return name.Trim();
        }

        private static int ValidateDisplayOrder(int displayOrder)
        {
            if (displayOrder < 0)
                throw new ArgumentException("Display order cannot be negative.", nameof(displayOrder));

            return displayOrder;
        }
    }
}
