using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Products;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Products;
using Microsoft.Extensions.Logging;

namespace RewardPointsSystem.Application.Services.Products
{
    /// <summary>
    /// Service: CategoryService
    /// Responsibility: Manage product categories
    /// Clean Architecture - Application layer encapsulates category business logic
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.ProductCategories.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            return categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => MapToDto(c, products.Count(p => p.CategoryId == c.Id && p.IsActive)));
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.ProductCategories.GetAllAsync();
            var products = await _unitOfWork.Products.GetAllAsync();

            return categories
                .OrderBy(c => c.DisplayOrder)
                .Select(c => MapToDto(c, products.Count(p => p.CategoryId == c.Id)));
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid categoryId)
        {
            var category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryId);
            if (category == null) return null;

            var products = await _unitOfWork.Products.GetAllAsync();
            return MapToDto(category, products.Count(p => p.CategoryId == category.Id));
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Category name is required");

            // Check for duplicate name
            var existingCategories = await _unitOfWork.ProductCategories.GetAllAsync();
            if (existingCategories.Any(c => c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"A category with name '{dto.Name}' already exists");

            var category = ProductCategory.Create(dto.Name, dto.DisplayOrder, dto.Description);

            await _unitOfWork.ProductCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(category, 0);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto)
        {
            var category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            // Check for duplicate name if name is being changed
            if (!string.IsNullOrWhiteSpace(dto.Name) && 
                !dto.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingCategories = await _unitOfWork.ProductCategories.GetAllAsync();
                if (existingCategories.Any(c => c.Id != categoryId && 
                    c.Name.Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"A category with name '{dto.Name}' already exists");
                }
            }

            // Update category info
            category.UpdateInfo(
                dto.Name ?? category.Name,
                dto.DisplayOrder ?? category.DisplayOrder,
                dto.Description ?? category.Description
            );

            var products = await _unitOfWork.Products.GetAllAsync();

            // Handle IsActive changes
            if (dto.IsActive.HasValue && dto.IsActive.Value != category.IsActive)
            {
                if (dto.IsActive.Value)
                {
                    category.Activate();
                }
                else
                {
                    category.Deactivate();

                    // Set products in this category to uncategorized
                    var productsInCategory = products.Where(p => p.CategoryId == category.Id).ToList();
                    foreach (var product in productsInCategory)
                    {
                        product.UpdateCategory(null);
                    }

                    if (productsInCategory.Any())
                    {
                        _logger.LogInformation(
                            "Set {Count} product(s) to uncategorized when deactivating category '{Name}'",
                            productsInCategory.Count, category.Name);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(category, products.Count(p => p.CategoryId == category.Id));
        }

        public async Task<string> DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found");

            var products = await _unitOfWork.Products.GetAllAsync();
            var productsInCategory = products.Where(p => p.CategoryId == categoryId).ToList();

            // Set products to uncategorized
            foreach (var product in productsInCategory)
            {
                product.UpdateCategory(null);
            }

            if (productsInCategory.Any())
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                    "Set {Count} product(s) to uncategorized before deleting category '{Name}'",
                    productsInCategory.Count, category.Name);
            }

            await _unitOfWork.ProductCategories.DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return productsInCategory.Any()
                ? $"Category '{category.Name}' deleted successfully. {productsInCategory.Count} product(s) are now uncategorized."
                : $"Category '{category.Name}' deleted successfully";
        }

        private static CategoryResponseDto MapToDto(ProductCategory category, int productCount)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                ProductCount = productCount
            };
        }
    }
}
