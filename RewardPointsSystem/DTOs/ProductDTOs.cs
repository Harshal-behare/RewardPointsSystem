using System;

namespace RewardPointsSystem.DTOs
{
    public class ProductUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for inventory alerts - Architecture Compliant
    /// </summary>
    public class InventoryAlert
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public string AlertType { get; set; }
    }
}