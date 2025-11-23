using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs.Products
{
    /// <summary>
    /// DTO for creating a product
    /// </summary>
    public class CreateProductDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public int PointsPrice { get; set; }
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// Basic product response DTO
    /// </summary>
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public int CurrentPointsCost { get; set; }
        public bool IsActive { get; set; }
        public bool IsInStock { get; set; }
        public int StockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed product response with pricing and inventory
    /// </summary>
    public class ProductDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public int CurrentPointsCost { get; set; }
        public bool IsActive { get; set; }
        public bool IsInStock { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PricingHistoryDto> PricingHistory { get; set; }
    }

    /// <summary>
    /// DTO for setting product pricing
    /// </summary>
    public class SetPricingDto
    {
        public Guid ProductId { get; set; }
        public int PointsCost { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

    /// <summary>
    /// DTO for updating inventory
    /// </summary>
    public class UpdateInventoryDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
    }

    /// <summary>
    /// DTO for inventory response
    /// </summary>
    public class InventoryResponseDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int QuantityOnHand { get; set; }
        public int ReorderLevel { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for pricing history
    /// </summary>
    public class PricingHistoryDto
    {
        public int PointsCost { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
