using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Products;
using RewardPointsSystem.Models.Operations;
using RewardPointsSystem.DTOs;

namespace RewardPointsSystem.Services.Products
{
    /// <summary>
    /// Service: ProductCatalogService
    /// Responsibility: Manage product information only
    /// </summary>
    public class ProductCatalogService : IProductCatalogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductCatalogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Product> CreateProductAsync(string name, string description, string category)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required", nameof(name));
            
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Product description is required", nameof(description));
            
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Product category is required", nameof(category));

            var product = new Product
            {
                Name = name.Trim(),
                Description = description.Trim(),
                Category = category.Trim(),
                ImageUrl = "", // Default empty
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty // TODO: Get from current user context
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            
            return product;
        }

        public async Task<Product> UpdateProductAsync(Guid id, ProductUpdateDto updates)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException($"Product with ID {id} not found", nameof(id));

            if (!string.IsNullOrWhiteSpace(updates.Name))
                product.Name = updates.Name.Trim();
            
            if (!string.IsNullOrWhiteSpace(updates.Description))
                product.Description = updates.Description.Trim();
            
            if (!string.IsNullOrWhiteSpace(updates.Category))
                product.Category = updates.Category.Trim();
            
            if (!string.IsNullOrWhiteSpace(updates.ImageUrl))
                product.ImageUrl = updates.ImageUrl.Trim();

            await _unitOfWork.SaveChangesAsync();
            return product;
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Where(p => p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required", nameof(category));

            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Where(p => p.IsActive && 
                                      p.Category.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task DeactivateProductAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException($"Product with ID {id} not found", nameof(id));

            // Check for pending redemptions before deactivating
            var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
            var hasPendingRedemptions = redemptions.Any(r => r.ProductId == id && 
                                                           r.Status == RedemptionStatus.Pending);

            if (hasPendingRedemptions)
                throw new InvalidOperationException("Cannot deactivate product with pending redemptions");

            product.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}