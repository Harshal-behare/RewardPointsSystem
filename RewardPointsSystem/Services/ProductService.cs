using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly List<Product> _products = new();

        public void AddProduct(Product product)
        {
            if (product.RequiredPoints <= 0) throw new ArgumentException("Points must be positive");
            if (product.Stock < 0) throw new ArgumentException("Stock cannot be negative");

            _products.Add(product);
        }

        public IEnumerable<Product> GetAllProducts() => _products;

        public Product GetProductById(Guid id) => _products.FirstOrDefault(p => p.Id == id);
    }
}

