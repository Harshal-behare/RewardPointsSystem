using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Products;
using RewardPointsSystem.Models.Operations;

namespace RewardPointsSystem.Services.Products
{
    public class ProductCatalogService : IProductCatalogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductCatalogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<Product> CreateProductAsync(string name, string description, string category)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Product description is required", nameof(description));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Product category is required", nameof(category));

            // Check for duplicate product name
            var existingProduct = await _unitOfWork.Products.SingleOrDefaultAsync(p => p.Name == name && p.IsActive);
            if (existingProduct != null)
                throw new InvalidOperationException($"Active product with name '{name}' already exists");

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Category = category,
                ImageUrl = string.Empty,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(Guid id, ProductUpdateDto updates)
        {
            if (updates == null)
                throw new ArgumentNullException(nameof(updates));

            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {id} not found");

            // Check for name uniqueness if name is being updated
            if (!string.IsNullOrWhiteSpace(updates.Name) && updates.Name != product.Name)
            {
                var existingProduct = await _unitOfWork.Products.SingleOrDefaultAsync(p => p.Name == updates.Name && p.IsActive && p.Id != id);
                if (existingProduct != null)
                    throw new InvalidOperationException($"Active product with name '{updates.Name}' already exists");

                product.Name = updates.Name;
            }

            if (!string.IsNullOrWhiteSpace(updates.Description))
                product.Description = updates.Description;

            if (!string.IsNullOrWhiteSpace(updates.Category))
                product.Category = updates.Category;

            if (!string.IsNullOrWhiteSpace(updates.ImageUrl))
                product.ImageUrl = updates.ImageUrl;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product;
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _unitOfWork.Products.FindAsync(p => p.IsActive);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required", nameof(category));

            return await _unitOfWork.Products.FindAsync(p => p.IsActive && p.Category == category);
        }

        public async Task<Product> GetProductAsync(Guid id)
        {
            return await _unitOfWork.Products.GetByIdAsync(id);
        }

        public async Task DeactivateProductAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {id} not found");

            // Check if product has pending redemptions
            var pendingRedemptions = await _unitOfWork.Redemptions.FindAsync(r => r.ProductId == id && r.Status == RedemptionStatus.Pending);
            if (pendingRedemptions.Any())
                throw new InvalidOperationException($"Cannot deactivate product '{product.Name}' as it has pending redemptions");

            product.IsActive = false;

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            var products = await _unitOfWork.Products.FindAsync(p => p.IsActive);
            var categories = new HashSet<string>();

            foreach (var product in products)
            {
                if (!string.IsNullOrWhiteSpace(product.Category))
                {
                    categories.Add(product.Category);
                }
            }

            return categories;
        }

        public async Task<int> GetProductCountByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required", nameof(category));

            return await _unitOfWork.Products.CountAsync(p => p.IsActive && p.Category == category);
        }

        public async Task<bool> IsProductActiveAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product?.IsActive == true;
        }
    }
}