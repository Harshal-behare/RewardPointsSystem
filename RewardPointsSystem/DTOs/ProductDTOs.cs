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

    public class TransactionSummaryDto
    {
        public Guid UserId { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public int CurrentBalance { get; set; }
        public int EarnedTransactionCount { get; set; }
        public int RedeemedTransactionCount { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class DashboardStats
    {
        public int TotalActiveUsers { get; set; }
        public int TotalActiveEvents { get; set; }
        public int TotalActiveProducts { get; set; }
        public int TotalPointsInCirculation { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class InventoryAlert
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public string AlertType { get; set; } // LowStock, OutOfStock
    }

    public class PointsSummary
    {
        public int TotalPointsAwarded { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int TotalPointsInCirculation { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}