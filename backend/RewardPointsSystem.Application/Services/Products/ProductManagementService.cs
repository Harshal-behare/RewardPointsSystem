using Microsoft.Extensions.Logging;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Products
{
    /// <summary>
    /// Service implementation for product management operations.
    /// All business logic centralized here - Clean Architecture compliant.
    /// </summary>
    public class ProductManagementService : IProductManagementService
    {
        private readonly IProductCatalogService _productService;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductManagementService> _logger;

        public ProductManagementService(
            IProductCatalogService productService,
            IPricingService pricingService,
            IInventoryService inventoryService,
            ILogger<ProductManagementService> logger)
        {
            _productService = productService;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ProductOperationResult> CreateProductAsync(CreateProductDto dto, Guid userId)
        {
            try
            {
                // Create the product
                var product = await _productService.CreateProductAsync(dto, userId);

                // Create pricing record if points price is provided
                if (dto.PointsPrice > 0)
                {
                    await _pricingService.SetProductPointsCostAsync(product.Id, dto.PointsPrice, DateTime.UtcNow);
                }

                // Create inventory record - always create one to ensure the record exists
                var stockQuantity = dto.StockQuantity > 0 ? dto.StockQuantity : 0;
                await _inventoryService.CreateInventoryAsync(product.Id, stockQuantity, 10); // Default reorder level of 10

                // Build response DTO
                var productDto = new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    CategoryId = product.CategoryId,
                    CategoryName = product.ProductCategory?.Name,
                    ImageUrl = product.ImageUrl,
                    CurrentPointsCost = dto.PointsPrice,
                    IsActive = product.IsActive,
                    IsInStock = dto.StockQuantity > 0,
                    StockQuantity = dto.StockQuantity,
                    CreatedAt = product.CreatedAt
                };

                _logger.LogInformation("Product {ProductName} created by user {UserId}", product.Name, userId);
                return ProductOperationResult.Succeeded(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ProductOperationResult> UpdateProductAsync(Guid id, UpdateProductDto dto, Guid userId)
        {
            try
            {
                _logger.LogInformation("UpdateProduct called for ID: {ProductId} by User: {UserId}", id, userId);

                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return ProductOperationResult.Failed(
                        $"Product with ID {id} not found",
                        ProductOperationErrorType.NotFound);
                }

                _logger.LogInformation("Existing product found: Name={Name}, IsActive={Active}", 
                    existingProduct.Name, existingProduct.IsActive);

                // Build update data with defaults from existing product
                var updateData = new ProductUpdateDto
                {
                    Name = dto.Name ?? existingProduct.Name,
                    Description = dto.Description ?? existingProduct.Description,
                    ImageUrl = dto.ImageUrl ?? existingProduct.ImageUrl
                };

                _logger.LogInformation("Calling UpdateProductAsync...");
                var product = await _productService.UpdateProductAsync(id, updateData);
                _logger.LogInformation("UpdateProductAsync completed successfully");

                // Update CategoryId if provided
                if (dto.CategoryId.HasValue && dto.CategoryId.Value != existingProduct.CategoryId)
                {
                    _logger.LogInformation("Updating category to {CategoryId}", dto.CategoryId.Value);
                    try
                    {
                        await _productService.UpdateProductCategoryAsync(id, dto.CategoryId.Value);
                        _logger.LogInformation("Category updated successfully");
                    }
                    catch (KeyNotFoundException)
                    {
                        // Category doesn't exist, skip update
                        _logger.LogWarning("Category {CategoryId} not found, skipping category update", dto.CategoryId.Value);
                    }
                }

                // Update active status if provided
                if (dto.IsActive.HasValue && dto.IsActive.Value != existingProduct.IsActive)
                {
                    _logger.LogInformation("Updating IsActive from {Old} to {New}", existingProduct.IsActive, dto.IsActive.Value);
                    await _productService.SetProductActiveStatusAsync(id, dto.IsActive.Value);
                    _logger.LogInformation("IsActive updated successfully");
                }

                // Update pricing if provided and greater than 0
                if (dto.PointsPrice.HasValue && dto.PointsPrice.Value > 0)
                {
                    _logger.LogInformation("Updating pricing to {Price}", dto.PointsPrice.Value);
                    await _pricingService.UpdatePointsCostAsync(id, dto.PointsPrice.Value);
                    _logger.LogInformation("Pricing updated successfully");
                }

                // Update inventory if provided
                if (dto.StockQuantity.HasValue)
                {
                    _logger.LogInformation("Updating stock quantity to {Quantity}", dto.StockQuantity.Value);
                    await _inventoryService.AdjustStockAsync(id, dto.StockQuantity.Value, userId);
                    _logger.LogInformation("Stock updated successfully");
                }

                _logger.LogInformation("Fetching final product data for response...");
                
                // Get all updated data for response
                var price = await _pricingService.GetCurrentPointsCostAsync(id);
                var inStock = await _inventoryService.IsInStockAsync(id);
                var stockQuantity = await _inventoryService.GetAvailableStockAsync(id);

                // Get updated product data
                var updatedProduct = await _productService.GetProductByIdAsync(id);
                var productDto = new ProductResponseDto
                {
                    Id = id,
                    Name = updatedProduct?.Name ?? product.Name,
                    Description = updatedProduct?.Description ?? product.Description,
                    CategoryId = updatedProduct?.CategoryId ?? product.CategoryId,
                    CategoryName = updatedProduct?.ProductCategory?.Name ?? product.ProductCategory?.Name,
                    ImageUrl = updatedProduct?.ImageUrl ?? product.ImageUrl,
                    CurrentPointsCost = price,
                    IsActive = updatedProduct?.IsActive ?? product.IsActive,
                    IsInStock = inStock,
                    StockQuantity = stockQuantity,
                    CreatedAt = updatedProduct?.CreatedAt ?? product.CreatedAt
                };

                _logger.LogInformation("Product update completed successfully");
                return ProductOperationResult.Succeeded(productDto);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "KeyNotFoundException for product {ProductId}", id);
                return ProductOperationResult.Failed(
                    $"Product with ID {id} not found",
                    ProductOperationErrorType.NotFound);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException while updating product {ProductId}: {Message}", id, ex.Message);
                return ProductOperationResult.Failed(ex.Message, ProductOperationErrorType.ValidationError);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ArgumentException while updating product {ProductId}: {Message}", id, ex.Message);
                return ProductOperationResult.Failed(ex.Message, ProductOperationErrorType.ValidationError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating product {ProductId}: {Type} - {Message}", 
                    id, ex.GetType().Name, ex.Message);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<ProductOperationResult> DeactivateProductAsync(Guid id)
        {
            try
            {
                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return ProductOperationResult.Failed(
                        $"Product with ID {id} not found",
                        ProductOperationErrorType.NotFound);
                }

                await _productService.DeactivateProductAsync(id);

                _logger.LogInformation("Product {ProductId} deactivated successfully", id);
                return ProductOperationResult.Succeeded();
            }
            catch (KeyNotFoundException)
            {
                return ProductOperationResult.Failed(
                    $"Product with ID {id} not found",
                    ProductOperationErrorType.NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating product {ProductId}", id);
                throw;
            }
        }
    }
}
