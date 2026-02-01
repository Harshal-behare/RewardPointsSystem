using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Products;

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
        /// Get all active products (for employees/general use)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                // Load only active products for general users
                var products = await _unitOfWork.Products.FindWithIncludesAsync(
                    p => p.IsActive,
                    p => p.ProductCategory
                );
                var allInventory = await _unitOfWork.Inventory.GetAllAsync();
                var productDtos = new List<ProductResponseDto>();

                foreach (var product in products)
                {
                    var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                    var inStock = await _inventoryService.IsInStockAsync(product.Id);
                    var inventory = allInventory.FirstOrDefault(i => i.ProductId == product.Id);
                    var stockQuantity = inventory != null ? inventory.QuantityAvailable - inventory.QuantityReserved : 0;

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

                return Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return Error("Failed to retrieve products");
            }
        }

        /// <summary>
        /// Get all products including inactive (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProductsAdmin()
        {
            try
            {
                // Load ALL products for admin (including inactive)
                var products = await _unitOfWork.Products.FindWithIncludesAsync(
                    p => true, // No filter - return all products
                    p => p.ProductCategory
                );
                var allInventory = await _unitOfWork.Inventory.GetAllAsync();
                var productDtos = new List<ProductResponseDto>();

                foreach (var product in products)
                {
                    var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                    var inStock = await _inventoryService.IsInStockAsync(product.Id);
                    var inventory = allInventory.FirstOrDefault(i => i.ProductId == product.Id);
                    var stockQuantity = inventory != null ? inventory.QuantityAvailable - inventory.QuantityReserved : 0;

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

                return Success(productDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products for admin");
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
                var inventory = await _unitOfWork.Inventory.SingleOrDefaultAsync(i => i.ProductId == id);
                var stockQuantity = inventory != null ? inventory.QuantityAvailable - inventory.QuantityReserved : 0;

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
                    StockQuantity = stockQuantity,
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

                // Create pricing record if points price is provided
                if (dto.PointsPrice > 0)
                {
                    await _pricingService.SetProductPointsCostAsync(product.Id, dto.PointsPrice, DateTime.UtcNow);
                }

                // Create inventory record if stock quantity is provided
                if (dto.StockQuantity > 0)
                {
                    await _inventoryService.CreateInventoryAsync(product.Id, dto.StockQuantity, 10); // Default reorder level of 10
                }
                else
                {
                    // Create inventory with 0 stock to ensure the record exists
                    await _inventoryService.CreateInventoryAsync(product.Id, 0, 10);
                }

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
                // Get authenticated user ID
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return UnauthorizedError("User not authenticated");

                _logger.LogInformation("UpdateProduct called for ID: {ProductId} by User: {UserId}", id, userId);
                _logger.LogInformation("DTO received: Name={Name}, Description={Desc}, CategoryId={CatId}, PointsPrice={Price}, StockQuantity={Stock}, IsActive={Active}, ImageUrl={Img}",
                    dto.Name, dto.Description?.Substring(0, Math.Min(50, dto.Description?.Length ?? 0)), 
                    dto.CategoryId, dto.PointsPrice, dto.StockQuantity, dto.IsActive, dto.ImageUrl);

                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFoundError($"Product with ID {id} not found");

                _logger.LogInformation("Existing product found: Name={Name}, IsActive={Active}", existingProduct.Name, existingProduct.IsActive);

                var updateData = new RewardPointsSystem.Application.DTOs.ProductUpdateDto
                {
                    Name = dto.Name ?? existingProduct.Name,
                    Description = dto.Description ?? existingProduct.Description,
                    Category = existingProduct.Category, // Keep existing category string (deprecated field)
                    ImageUrl = dto.ImageUrl ?? existingProduct.ImageUrl
                };

                _logger.LogInformation("Calling UpdateProductAsync...");
                var product = await _productService.UpdateProductAsync(id, updateData);
                _logger.LogInformation("UpdateProductAsync completed successfully");

                // Update CategoryId if provided
                if (dto.CategoryId.HasValue && dto.CategoryId.Value != existingProduct.CategoryId)
                {
                    _logger.LogInformation("Updating category to {CategoryId}", dto.CategoryId.Value);
                    // Verify the category exists
                    var category = await _unitOfWork.ProductCategories.GetByIdAsync(dto.CategoryId.Value);
                    if (category != null)
                    {
                        product.UpdateCategory(dto.CategoryId.Value);
                        await _unitOfWork.SaveChangesAsync();
                        _logger.LogInformation("Category updated successfully");
                    }
                }

                // Update active status if provided
                if (dto.IsActive.HasValue && dto.IsActive.Value != product.IsActive)
                {
                    _logger.LogInformation("Updating IsActive from {Old} to {New}", product.IsActive, dto.IsActive.Value);
                    if (dto.IsActive.Value)
                    {
                        product.Activate();
                    }
                    else
                    {
                        product.Deactivate();
                    }
                    await _unitOfWork.SaveChangesAsync();
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
                    // Check if inventory exists first
                    var existingInventory = await _unitOfWork.Inventory.SingleOrDefaultAsync(i => i.ProductId == id);
                    if (existingInventory != null)
                    {
                        // Update existing inventory - set the stock to the new value
                        var currentStock = existingInventory.QuantityAvailable;
                        var difference = dto.StockQuantity.Value - currentStock;
                        _logger.LogInformation("Current stock: {Current}, Difference: {Diff}", currentStock, difference);
                        if (difference != 0)
                        {
                            // Use Adjust method which handles both positive and negative changes
                            existingInventory.Adjust(difference, userId);
                            await _unitOfWork.SaveChangesAsync();
                            _logger.LogInformation("Stock adjusted successfully");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Creating new inventory for product");
                        // Create new inventory if it doesn't exist
                        await _inventoryService.CreateInventoryAsync(id, dto.StockQuantity.Value, 10);
                        _logger.LogInformation("Inventory created successfully");
                    }
                }

                _logger.LogInformation("Fetching final product data for response...");
                var price = await _pricingService.GetCurrentPointsCostAsync(product.Id);
                var inStock = await _inventoryService.IsInStockAsync(product.Id);
                
                // Get actual stock quantity
                var inventory = await _unitOfWork.Inventory.SingleOrDefaultAsync(i => i.ProductId == id);
                var stockQuantity = inventory != null ? inventory.QuantityAvailable - inventory.QuantityReserved : 0;

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
                    StockQuantity = stockQuantity,
                    CreatedAt = product.CreatedAt
                };

                _logger.LogInformation("Product update completed successfully");
                return Success(productDto, "Product updated successfully");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "KeyNotFoundException for product {ProductId}", id);
                return NotFoundError($"Product with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException while updating product {ProductId}: {Message}", id, ex.Message);
                return Error(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "ArgumentException while updating product {ProductId}: {Message}", id, ex.Message);
                return Error(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating product {ProductId}: {Type} - {Message}", id, ex.GetType().Name, ex.Message);
                return Error($"Failed to update product: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivate a product (Admin only). Products cannot be permanently deleted, only deactivated.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <response code="200">Product deactivated successfully</response>
        /// <response code="404">Product not found</response>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateProduct(Guid id)
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

        /// <summary>
        /// Get all product categories
        /// </summary>
        /// <response code="200">Returns all product categories</response>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.ProductCategories.GetAllAsync();
                var activeCat = categories.Where(c => c.IsActive).OrderBy(c => c.DisplayOrder);
                
                // Get product counts for each category
                var products = await _unitOfWork.Products.GetAllAsync();
                
                var categoryDtos = activeCat.Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    ProductCount = products.Count(p => p.CategoryId == c.Id && p.IsActive)
                });

                return Success(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return Error("Failed to retrieve categories");
            }
        }

        /// <summary>
        /// Get all categories including inactive (Admin only)
        /// </summary>
        /// <response code="200">Returns all categories including inactive</response>
        [HttpGet("categories/admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesAdmin()
        {
            try
            {
                var categories = await _unitOfWork.ProductCategories.GetAllAsync();
                var products = await _unitOfWork.Products.GetAllAsync();
                
                var categoryDtos = categories.OrderBy(c => c.DisplayOrder).Select(c => new CategoryResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder,
                    IsActive = c.IsActive,
                    ProductCount = products.Count(p => p.CategoryId == c.Id)
                });

                return Success(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all categories for admin");
                return Error("Failed to retrieve categories");
            }
        }

        /// <summary>
        /// Create a new product category (Admin only)
        /// </summary>
        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                // Validate name is not empty
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return ValidationError(new[] { "Category name is required" });

                // Check if category with same name already exists
                var existingCategories = await _unitOfWork.ProductCategories.GetAllAsync();
                if (existingCategories.Any(c => c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    return ValidationError(new[] { $"A category with name '{dto.Name}' already exists" });

                var category = ProductCategory.Create(dto.Name, dto.DisplayOrder, dto.Description);
                
                await _unitOfWork.ProductCategories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var response = new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive,
                    ProductCount = 0
                };

                return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, 
                    new ApiResponse<CategoryResponseDto> { Success = true, Data = response, Message = "Category created successfully" });
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new[] { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return Error("Failed to create category");
            }
        }

        /// <summary>
        /// Update a product category (Admin only)
        /// </summary>
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
                if (category == null)
                    return NotFoundError($"Category with ID {id} not found");

                // Check for duplicate name if name is being changed
                if (!string.IsNullOrWhiteSpace(dto.Name) && !dto.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
                {
                    var existingCategories = await _unitOfWork.ProductCategories.GetAllAsync();
                    if (existingCategories.Any(c => c.Id != id && c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                        return ValidationError(new[] { $"A category with name '{dto.Name}' already exists" });
                }

                // Update category info
                category.UpdateInfo(
                    dto.Name ?? category.Name,
                    dto.DisplayOrder ?? category.DisplayOrder,
                    dto.Description ?? category.Description
                );

                // Fetch products for both deactivation logic and response
                var products = await _unitOfWork.Products.GetAllAsync();

                // Handle IsActive changes
                if (dto.IsActive.HasValue && dto.IsActive.Value != category.IsActive)
                {
                    if (dto.IsActive.Value)
                    {
                        category.Activate();
                    }
                    else
                    {
                        category.Deactivate();
                        
                        // Set products in this category to uncategorized when category is deactivated
                        var productsInCategory = products.Where(p => p.CategoryId == category.Id).ToList();
                        foreach (var product in productsInCategory)
                        {
                            product.UpdateCategory(null); // Set to uncategorized
                        }
                        
                        if (productsInCategory.Any())
                        {
                            _logger.LogInformation("Set {Count} product(s) to uncategorized when deactivating category '{Name}'", 
                                productsInCategory.Count, category.Name);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                var response = new CategoryResponseDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    DisplayOrder = category.DisplayOrder,
                    IsActive = category.IsActive,
                    ProductCount = products.Count(p => p.CategoryId == category.Id)
                };

                return Success(response, "Category updated successfully");
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new[] { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new[] { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {CategoryId}", id);
                return Error("Failed to update category");
            }
        }

        /// <summary>
        /// Delete a product category (Admin only)
        /// </summary>
        /// <remarks>
        /// This will permanently delete the category. Products in this category will become "Uncategorized" (CategoryId set to null).
        /// </remarks>
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var category = await _unitOfWork.ProductCategories.GetByIdAsync(id);
                if (category == null)
                    return NotFoundError($"Category with ID {id} not found");

                // Find all products using this category and set them to uncategorized
                var products = await _unitOfWork.Products.GetAllAsync();
                var productsInCategory = products.Where(p => p.CategoryId == id).ToList();

                foreach (var product in productsInCategory)
                {
                    product.UpdateCategory(null); // Set to uncategorized
                }
                
                if (productsInCategory.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Set {Count} product(s) to uncategorized before deleting category '{Name}'", 
                        productsInCategory.Count, category.Name);
                }

                await _unitOfWork.ProductCategories.DeleteAsync(category);
                await _unitOfWork.SaveChangesAsync();

                var message = productsInCategory.Any() 
                    ? $"Category '{category.Name}' deleted successfully. {productsInCategory.Count} product(s) are now uncategorized."
                    : $"Category '{category.Name}' deleted successfully";

                return Success<object>(null, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category {CategoryId}", id);
                return Error("Failed to delete category");
            }
        }
    }
}
