using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Products
{
    /// <summary>
    /// Service: ProductQueryService
    /// Responsibility: Query product data with related entities (read operations)
    /// Clean Architecture Compliant - Encapsulates data access logic
    /// </summary>
    public class ProductQueryService : IProductQueryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;

        public ProductQueryService(
            IUnitOfWork unitOfWork,
            IPricingService pricingService,
            IInventoryService inventoryService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        public async Task<IEnumerable<ProductResponseDto>> GetActiveProductsWithDetailsAsync()
        {
            var products = await _unitOfWork.Products.FindWithIncludesAsync(
                p => p.IsActive,
                p => p.ProductCategory);
            
            return await MapProductsToResponseDtosAsync(products);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsWithDetailsAsync()
        {
            var products = await _unitOfWork.Products.FindWithIncludesAsync(
                p => true,
                p => p.ProductCategory);
            
            return await MapProductsToResponseDtosAsync(products);
        }

        public async Task<ProductResponseDto?> GetProductWithDetailsAsync(Guid productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return null;

            // Get category
            string? categoryName = null;
            if (product.CategoryId.HasValue)
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(product.CategoryId.Value);
                categoryName = category?.Name;
            }

            var price = await _pricingService.GetCurrentPointsCostAsync(productId);
            var inStock = await _inventoryService.IsInStockAsync(productId);
            var inventory = await _unitOfWork.Inventory.SingleOrDefaultAsync(i => i.ProductId == productId);
            var stockQuantity = inventory != null ? inventory.QuantityAvailable - inventory.QuantityReserved : 0;

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                ImageUrl = product.ImageUrl,
                CurrentPointsCost = price,
                IsActive = product.IsActive,
                IsInStock = inStock,
                StockQuantity = stockQuantity,
                CreatedAt = product.CreatedAt
            };
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryWithDetailsAsync(Guid categoryId)
        {
            var products = await _unitOfWork.Products.FindWithIncludesAsync(
                p => p.CategoryId == categoryId && p.IsActive,
                p => p.ProductCategory);
            
            return await MapProductsToResponseDtosAsync(products);
        }

        private async Task<IEnumerable<ProductResponseDto>> MapProductsToResponseDtosAsync(
            IEnumerable<Domain.Entities.Products.Product> products)
        {
            var inventory = await _unitOfWork.Inventory.GetAllAsync();
            var productDtos = new List<ProductResponseDto>();

            foreach (var product in products)
            {
                var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                var inStock = await _inventoryService.IsInStockAsync(product.Id);
                var inventoryItem = inventory.FirstOrDefault(i => i.ProductId == product.Id);
                var stockQuantity = inventoryItem != null ? inventoryItem.QuantityAvailable - inventoryItem.QuantityReserved : 0;

                productDtos.Add(new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    CategoryName = product.ProductCategory?.Name,
                    ImageUrl = product.ImageUrl,
                    CurrentPointsCost = price,
                    IsActive = product.IsActive,
                    IsInStock = inStock,
                    StockQuantity = stockQuantity,
                    CreatedAt = product.CreatedAt
                });
            }

            return productDtos;
        }
    }
}
