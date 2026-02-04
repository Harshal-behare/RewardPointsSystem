using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: ICategoryService
    /// Responsibility: Manage product categories
    /// Clean Architecture - Encapsulates category business logic
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Get all active categories with product counts
        /// </summary>
        Task<IEnumerable<CategoryResponseDto>> GetActiveCategoriesAsync();
        
        /// <summary>
        /// Get all categories including inactive (admin view)
        /// </summary>
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        
        /// <summary>
        /// Get category by ID
        /// </summary>
        Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid categoryId);
        
        /// <summary>
        /// Create a new category
        /// </summary>
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);
        
        /// <summary>
        /// Update an existing category
        /// </summary>
        Task<CategoryResponseDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto);
        
        /// <summary>
        /// Delete a category (products become uncategorized)
        /// </summary>
        Task<string> DeleteCategoryAsync(Guid categoryId);
    }
}
