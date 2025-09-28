using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Products;

namespace RewardPointsSystem.Interfaces
{
    public interface IProductCatalogService
    {
        Task<Product> CreateProductAsync(string name, string description, string category);
        Task<Product> UpdateProductAsync(Guid id, ProductUpdateDto updates);
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task DeactivateProductAsync(Guid id);
    }

    public class ProductUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
    }
}