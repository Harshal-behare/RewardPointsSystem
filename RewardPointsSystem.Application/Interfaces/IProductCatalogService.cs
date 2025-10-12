using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IProductCatalogService
    /// Responsibility: Manage product information only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IProductCatalogService
    {
        Task<Product> CreateProductAsync(string name, string description, string category);
        Task<Product> UpdateProductAsync(Guid id, ProductUpdateDto updates);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task DeactivateProductAsync(Guid id);
    }
}