using System;
using System.Collections.Generic;
using System.Linq;
using RewardPointsSystem.Models;
using RewardPointsSystem.Interfaces;

namespace RewardPointsSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryService _inventoryService;

        public ProductService(IUnitOfWork unitOfWork, IInventoryService inventoryService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Validate that product doesn't already exist
            if (_unitOfWork.Products.Any(p => p.Name.Equals(product.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Product with name '{product.Name}' already exists");

            _unitOfWork.Products.Add(product);
            _unitOfWork.Complete();
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _unitOfWork.Products.Find(p => p.IsActive);
        }

        public Product GetProductById(Guid id)
        {
            return _unitOfWork.Products.GetById(id);
        }

        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            _unitOfWork.Products.Update(product);
            _unitOfWork.Complete();
        }

        public void DeactivateProduct(Guid productId)
        {
            var product = GetProductById(productId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} not found");

            product.IsActive = false;
            _unitOfWork.Products.Update(product);
            _unitOfWork.Complete();
        }

        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            return _unitOfWork.Products.Find(p => p.IsActive && p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }
    }
}

