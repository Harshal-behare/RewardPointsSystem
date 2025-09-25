using System;
using System.Collections.Generic;
using RewardPointsSystem.Models;

namespace RewardPointsSystem.Interfaces
{
    public interface IProductService
    {
        void AddProduct(Product product);
        Product GetProductById(Guid id);
        IEnumerable<Product> GetAllProducts();
        void UpdateProduct(Product product);
        void DeactivateProduct(Guid productId);
        IEnumerable<Product> GetProductsByCategory(string category);
    }
}

