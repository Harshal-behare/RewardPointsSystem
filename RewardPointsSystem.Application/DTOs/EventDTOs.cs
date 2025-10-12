using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs
{
    /// <summary>
    /// DTO for creating new events - Architecture Compliant
    /// </summary>
    public class CreateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int TotalPointsPool { get; set; }
    }

    /// <summary>
    /// DTO for updating existing events - Architecture Compliant
    /// </summary>
    public class UpdateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int TotalPointsPool { get; set; }
    }

    /// <summary>
    /// DTO for bulk awarding points to winners - Architecture Compliant
    /// </summary>
    public class WinnerDto
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public int Position { get; set; }
    }

    /// <summary>
    /// DTO for transaction summaries - Architecture Compliant
    /// </summary>
    public class TransactionSummaryDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public int CurrentBalance { get; set; }
        public DateTime LastActivity { get; set; }
    }

    /// <summary>
    /// DTO for admin dashboard statistics - Architecture Compliant
    /// </summary>
    public class DashboardStats
    {
        public int TotalUsers { get; set; }
        public int ActiveEvents { get; set; }
        public int PendingRedemptions { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public Dictionary<string, int> UserEventParticipation { get; set; }
        public Dictionary<string, int> UserPointsEarned { get; set; }
    }

    /// <summary>
    /// DTO for points summary reporting - Architecture Compliant
    /// </summary>
    public class PointsSummary
    {
        public int TotalPointsInCirculation { get; set; }
        public int TotalPointsAwarded { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public Dictionary<string, int> EventParticipationCounts { get; set; }
        public Dictionary<string, int> EventPointsAwarded { get; set; }
        public Dictionary<string, int> ProductRedemptionCounts { get; set; }
        public Dictionary<string, int> ProductRedemptionValues { get; set; }
        public Dictionary<string, int> CurrentStock { get; set; }
        public IEnumerable<object> LowStockProducts { get; set; }
        public IEnumerable<object> OutOfStockProducts { get; set; }
    }
}
