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
        public DateTime? EventEndDate { get; set; }
        public int TotalPointsPool { get; set; }
        public int? MaxParticipants { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public string? Location { get; set; }
        public string? VirtualLink { get; set; }
        public string? BannerImageUrl { get; set; }
        
        // Prize distribution for ranks
        public int? FirstPlacePoints { get; set; }
        public int? SecondPlacePoints { get; set; }
        public int? ThirdPlacePoints { get; set; }
    }

    /// <summary>
    /// DTO for updating existing events - Architecture Compliant
    /// </summary>
    public class UpdateEventDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public DateTime? EventEndDate { get; set; }
        public int? TotalPointsPool { get; set; }
        public string? Status { get; set; }  // Added: Draft, Upcoming, Active, Completed
        public int? MaxParticipants { get; set; }
        public DateTime? RegistrationStartDate { get; set; }
        public DateTime? RegistrationEndDate { get; set; }
        public string? Location { get; set; }
        public string? VirtualLink { get; set; }
        public string? BannerImageUrl { get; set; }
        
        // Prize distribution for ranks
        public int? FirstPlacePoints { get; set; }
        public int? SecondPlacePoints { get; set; }
        public int? ThirdPlacePoints { get; set; }
    }

    /// <summary>
    /// DTO for bulk awarding points to winners - Architecture Compliant
    /// </summary>
    public class WinnerDto
    {
        public Guid UserId { get; set; }
        public int Points { get; set; }
        public int EventRank { get; set; }
    }

    /// <summary>
    /// DTO for bulk awarding winners endpoint
    /// </summary>
    public class BulkAwardWinnersDto
    {
        public List<WinnerDto> Awards { get; set; } = new();
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
        public int TotalActiveUsers { get; set; }
        public int TotalEvents { get; set; }
        public int ActiveEvents { get; set; }
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int PendingRedemptions { get; set; }
        public int TotalRedemptions { get; set; }
        public int TotalPointsAwarded { get; set; }
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

    /// <summary>
    /// DTO for employee dashboard data - per dashboard.txt requirements
    /// </summary>
    public class EmployeeDashboardDto
    {
        public MyPointsDto MyPoints { get; set; }
        public MyRedemptionsDto MyRedemptions { get; set; }
        public AvailableEventsDto AvailableEvents { get; set; }
        public List<RecentActivityDto> RecentActivity { get; set; }
        public List<FeaturedProductDto> FeaturedProducts { get; set; }
    }

    public class MyPointsDto
    {
        public int CurrentBalance { get; set; }
        public int PendingPoints { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
    }

    public class MyRedemptionsDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Delivered { get; set; }
    }

    public class AvailableEventsDto
    {
        public List<EventSummaryDto> Upcoming { get; set; }
        public List<EventSummaryDto> Active { get; set; }
        public List<EventSummaryDto> MyRegistered { get; set; }
    }

    public class EventSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime EventDate { get; set; }
        public string Status { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class RecentActivityDto
    {
        public string Type { get; set; }  // "earned", "redeemed", "registered", "refund"
        public string Description { get; set; }
        public int? Points { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class FeaturedProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PointsCost { get; set; }
        public string ImageUrl { get; set; }
        public bool IsInStock { get; set; }
    }
}
