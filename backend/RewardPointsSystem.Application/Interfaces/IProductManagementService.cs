using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Service for product management operations with business logic.
    /// Part of Clean Architecture pattern - encapsulates all product manipulation logic.
    /// </summary>
    public interface IProductManagementService
    {
        /// <summary>
        /// Creates a new product with pricing and inventory
        /// </summary>
        Task<ProductOperationResult> CreateProductAsync(CreateProductDto dto, Guid userId);

        /// <summary>
        /// Updates an existing product including pricing, inventory, and category
        /// </summary>
        Task<ProductOperationResult> UpdateProductAsync(Guid id, UpdateProductDto dto, Guid userId);

        /// <summary>
        /// Deactivates a product (soft delete)
        /// </summary>
        Task<ProductOperationResult> DeactivateProductAsync(Guid id);
    }

    /// <summary>
    /// Result of a product operation
    /// </summary>
    public class ProductOperationResult
    {
        public bool Success { get; private set; }
        public string? ErrorMessage { get; private set; }
        public ProductOperationErrorType ErrorType { get; private set; }
        public ProductResponseDto? Data { get; private set; }

        private ProductOperationResult() { }

        public static ProductOperationResult Succeeded(ProductResponseDto? data = null)
        {
            return new ProductOperationResult
            {
                Success = true,
                Data = data
            };
        }

        public static ProductOperationResult Failed(string errorMessage, ProductOperationErrorType errorType = ProductOperationErrorType.ValidationError)
        {
            return new ProductOperationResult
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorType = errorType
            };
        }
    }

    public enum ProductOperationErrorType
    {
        ValidationError,
        NotFound,
        Conflict,
        Unauthorized
    }
}
