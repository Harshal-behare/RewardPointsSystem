using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages product catalog operations
    /// </summary>
    public class ProductsController : BaseApiController
    {
        private readonly IProductCatalogService _productService;
        private readonly IPricingService _pricingService;
        private readonly IInventoryService _inventoryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductCatalogService productService,
            IPricingService pricingService,
            IInventoryService inventoryService,
            IUnitOfWork unitOfWork,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _pricingService = pricingService;
            _inventoryService = inventoryService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetActiveProductsAsync();
                var productDtos = new List<ProductResponseDto>();

                foreach (var product in products)
                {
                    var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                    var inStock = await _inventoryService.IsInStockAsync(product.Id);

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
                        StockQuantity = 0,
                        CreatedAt = product.CreatedAt
                    });
                }

                return Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return Error("Failed to retrieve products");
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                    return NotFoundError($"Product with ID {id} not found");

                var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                var inStock = await _inventoryService.IsInStockAsync(product.Id);

                var productDto = new ProductResponseDto
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
                    StockQuantity = 0,
                    CreatedAt = product.CreatedAt
                };

                return Success(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return Error("Failed to retrieve product");
            }
        }

        /// <summary>
        /// Create a new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                // Get authenticated user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return UnauthorizedError("User not authenticated");

                var product = await _productService.CreateProductAsync(dto, userId);

                // TODO: Set pricing and inventory using separate endpoints
                // For now, product is created without pricing/inventory
                // Use POST /api/v1/pricing and POST /api/v1/inventory to set these separately

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

                return Created(productDto, "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return Error("Failed to create product");
            }
        }

        /// <summary>
        /// Update an existing product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="dto">Product update data</param>
        /// <response code="200">Product updated successfully</response>
        /// <response code="404">Product not found</response>
        /// <response code="422">Validation failed</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFoundError($"Product with ID {id} not found");

                var updateData = new RewardPointsSystem.Application.DTOs.ProductUpdateDto
                {
                    Name = dto.Name ?? existingProduct.Name,
                    Description = dto.Description ?? existingProduct.Description,
                    Category = dto.CategoryId?.ToString() ?? existingProduct.CategoryId?.ToString(),
                    ImageUrl = dto.ImageUrl ?? existingProduct.ImageUrl
                };

                var product = await _productService.UpdateProductAsync(id, updateData);

                // Update pricing if provided
                if (dto.PointsPrice.HasValue)
                {
                    await _pricingService.UpdatePointsCostAsync(id, dto.PointsPrice.Value);
                }

                // Update inventory if provided
                if (dto.StockQuantity.HasValue)
                {
                    await _inventoryService.AddStockAsync(id, dto.StockQuantity.Value);
                }

                var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                var inStock = await _inventoryService.IsInStockAsync(product.Id);

                var productDto = new ProductResponseDto
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
                    StockQuantity = dto.StockQuantity ?? 0,
                    CreatedAt = product.CreatedAt
                };

                return Success(productDto, "Product updated successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Product with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return Error("Failed to update product");
            }
        }

        /// <summary>
        /// Delete/Deactivate a product (Admin only)
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <response code="200">Product deactivated successfully</response>
        /// <response code="404">Product not found</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFoundError($"Product with ID {id} not found");

                await _productService.DeactivateProductAsync(id);

                return Success<object>(null, "Product deactivated successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Product with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return Error("Failed to delete product");
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <response code="200">Returns products in category</response>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                var productDtos = new List<ProductResponseDto>();

                foreach (var product in products)
                {
                    var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                    var inStock = await _inventoryService.IsInStockAsync(product.Id);

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
                        StockQuantity = 0,
                        CreatedAt = product.CreatedAt
                    });
                }

                return Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category {CategoryId}", categoryId);
                return Error("Failed to retrieve products");
            }
        }
    }
}
