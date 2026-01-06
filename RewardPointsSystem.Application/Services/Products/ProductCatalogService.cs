using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Services.Products
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

        public async Task<Product> CreateProductAsync(CreateProductDto dto, Guid createdBy)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Product name is required", nameof(dto));

            if (createdBy == Guid.Empty)
                throw new ArgumentException("Created by user ID is required", nameof(createdBy));

            // Create product
            var product = Product.Create(
                dto.Name.Trim(),
                createdBy,
                dto.Description?.Trim(),
                dto.CategoryId,
                dto.ImageUrl?.Trim());

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Note: Pricing and inventory are created separately via PricingService and InventoryService
            // This maintains single responsibility principle
            
            return product;
        }

        private async Task<User> GetOrCreateSystemUserAsync()
        {
            // Try to find system user
            var systemUser = await _unitOfWork.Users.SingleOrDefaultAsync(u => u.Email == "system@rewardpoints.com");
            
            if (systemUser == null)
            {
                // Create system user
                systemUser = User.Create(
                    "system@rewardpoints.com",
                    "System",
                    "Administrator");
                
                await _unitOfWork.Users.AddAsync(systemUser);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return systemUser;
        }

        public async Task<Product> UpdateProductAsync(Guid id, ProductUpdateDto updates)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException($"Product with ID {id} not found", nameof(id));

            var name = !string.IsNullOrWhiteSpace(updates.Name) ? updates.Name.Trim() : product.Name;
            var description = !string.IsNullOrWhiteSpace(updates.Description) ? updates.Description.Trim() : product.Description;
            var imageUrl = !string.IsNullOrWhiteSpace(updates.ImageUrl) ? updates.ImageUrl.Trim() : product.ImageUrl;

            product.UpdateDetails(name, description, product.CategoryId, imageUrl);

            await _unitOfWork.SaveChangesAsync();
            return product;
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Where(p => p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid? categoryId)
        {
            if (!categoryId.HasValue)
                throw new ArgumentException("Category ID is required", nameof(categoryId));

            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Where(p => p.IsActive && p.CategoryId == categoryId.Value);
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

            product.Deactivate();
            await _unitOfWork.SaveChangesAsync();
        }
    }
}