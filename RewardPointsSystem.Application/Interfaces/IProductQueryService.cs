using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IProductQueryService
    /// Responsibility: Query product data with related entities (read operations)
    /// Clean Architecture - Separates read concerns from IProductCatalogService
    /// </summary>
    public interface IProductQueryService
    {
        /// <summary>
        /// Get all active products with category, pricing, and inventory details
        /// </summary>
        Task<IEnumerable<ProductResponseDto>> GetActiveProductsWithDetailsAsync();
        
        /// <summary>
        /// Get all products including inactive (for admin view)
        /// </summary>
        Task<IEnumerable<ProductResponseDto>> GetAllProductsWithDetailsAsync();
        
        /// <summary>
        /// Get product details by ID with all related data
        /// </summary>
        Task<ProductResponseDto?> GetProductWithDetailsAsync(Guid productId);
        
        /// <summary>
        /// Get products by category with details
        /// </summary>
        Task<IEnumerable<ProductResponseDto>> GetProductsByCategoryWithDetailsAsync(Guid categoryId);
    }
}
