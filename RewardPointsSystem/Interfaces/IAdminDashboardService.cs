using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RewardPointsSystem.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<DashboardSummary> GetDashboardSummaryAsync();
        Task<UserActivityReport> GetUserActivityReportAsync(DateTime from, DateTime to);
        Task<EventParticipationReport> GetEventParticipationReportAsync(DateTime from, DateTime to);
        Task<RedemptionReport> GetRedemptionReportAsync(DateTime from, DateTime to);
        Task<InventoryReport> GetInventoryReportAsync();
    }

    public class DashboardSummary
    {
        public int TotalActiveUsers { get; set; }
        public int TotalActiveEvents { get; set; }
        public int TotalActiveProducts { get; set; }
        public decimal TotalPointsInCirculation { get; set; }
        public decimal TotalPointsAwarded { get; set; }
        public decimal TotalPointsRedeemed { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class UserActivityReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
        public Dictionary<Guid, int> UserEventParticipation { get; set; }
        public Dictionary<Guid, decimal> UserPointsEarned { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class EventParticipationReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Dictionary<Guid, int> EventParticipationCounts { get; set; }
        public Dictionary<Guid, decimal> EventPointsAwarded { get; set; }
        public int TotalParticipations { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class RedemptionReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Dictionary<Guid, int> ProductRedemptionCounts { get; set; }
        public Dictionary<Guid, decimal> ProductRedemptionValues { get; set; }
        public decimal TotalRedemptionValue { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class InventoryReport
    {
        public Dictionary<Guid, int> CurrentStock { get; set; }
        public List<Guid> LowStockProducts { get; set; }
        public List<Guid> OutOfStockProducts { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}