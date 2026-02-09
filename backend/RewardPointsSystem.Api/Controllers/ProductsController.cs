using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages product catalog operations.
    /// Simplified controller - most try-catch blocks removed as global exception handler
    /// will catch unhandled exceptions. Business errors use ProductOperationResult pattern.
    /// </summary>
    public class ProductsController : BaseApiController
    {
        private readonly IProductQueryService _productQueryService;
        private readonly IProductManagementService _productManagementService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductQueryService productQueryService,
            IProductManagementService productManagementService,
            ICategoryService categoryService,
            ILogger<ProductsController> logger)
        {
            _productQueryService = productQueryService;
            _productManagementService = productManagementService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active products (for employees/general use)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productQueryService.GetActiveProductsWithDetailsAsync();
            return Success(products);
        }

        /// <summary>
        /// Get all products including inactive (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProductsAdmin()
        {
            var products = await _productQueryService.GetAllProductsWithDetailsAsync();
            return Success(products);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductById(Guid id)
        {
            var product = await _productQueryService.GetProductWithDetailsAsync(id);
            if (product == null)
                return NotFoundError($"Product with ID {id} not found");

            return Success(product);
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
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return UnauthorizedError("User not authenticated");

            var result = await _productManagementService.CreateProductAsync(dto, userId.Value);
            if (!result.Success)
                return MapProductErrorToResponse(result);

            return Created(result.Data!, "Product created successfully");
        }

        /// <summary>
        /// Update an existing product (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return UnauthorizedError("User not authenticated");

            var result = await _productManagementService.UpdateProductAsync(id, dto, userId.Value);
            if (!result.Success)
                return MapProductErrorToResponse(result);

            return Success(result.Data!, "Product updated successfully");
        }

        /// <summary>
        /// Deactivate a product (Admin only). Products cannot be permanently deleted.
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeactivateProduct(Guid id)
        {
            var result = await _productManagementService.DeactivateProductAsync(id);
            if (!result.Success)
                return MapProductErrorToResponse(result);

            return Success<object>(null, "Product deactivated successfully");
        }

        /// <summary>
        /// Maps product operation result errors to appropriate HTTP responses
        /// </summary>
        private IActionResult MapProductErrorToResponse(ProductOperationResult result)
        {
            return result.ErrorType switch
            {
                ProductOperationErrorType.NotFound => NotFoundError(result.ErrorMessage!),
                ProductOperationErrorType.Conflict => ConflictError(result.ErrorMessage!),
                ProductOperationErrorType.Unauthorized => UnauthorizedError(result.ErrorMessage!),
                _ => Error(result.ErrorMessage!, 400)
            };
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsByCategory(Guid categoryId)
        {
            var products = await _productQueryService.GetProductsByCategoryWithDetailsAsync(categoryId);
            return Success(products);
        }

        /// <summary>
        /// Get all product categories
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return Success(categories);
        }

        /// <summary>
        /// Get all categories including inactive (Admin only)
        /// </summary>
        [HttpGet("categories/admin/all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesAdmin()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Success(categories);
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
                var category = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategories), new { id = category.Id },
                    new ApiResponse<CategoryResponseDto> { Success = true, Data = category, Message = "Category created successfully" });
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new[] { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new[] { ex.Message });
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
                var category = await _categoryService.UpdateCategoryAsync(id, dto);
                return Success(category, "Category updated successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Category with ID {id} not found");
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new[] { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new[] { ex.Message });
            }
        }

        /// <summary>
        /// Delete a product category (Admin only)
        /// </summary>
        /// <remarks>
        /// This will permanently delete the category. Products in this category will become "Uncategorized".
        /// </remarks>
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            try
            {
                var message = await _categoryService.DeleteCategoryAsync(id);
                return Success<object>(null, message);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Category with ID {id} not found");
            }
        }
    }
}
