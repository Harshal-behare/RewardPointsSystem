using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IProductCatalogService
    /// Responsibility: Manage product information only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IProductCatalogService
    {
        Task<Product> CreateProductAsync(CreateProductDto dto, Guid createdBy);
        Task<Product> UpdateProductAsync(Guid id, ProductUpdateDto updates);
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid? categoryId);
        Task DeactivateProductAsync(Guid id);
        Task ActivateProductAsync(Guid id);
        Task UpdateProductCategoryAsync(Guid productId, Guid categoryId);
        Task SetProductActiveStatusAsync(Guid productId, bool isActive);
    }
}