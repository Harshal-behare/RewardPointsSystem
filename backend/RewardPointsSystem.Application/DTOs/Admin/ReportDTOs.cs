using System;
using System.Collections.Generic;

namespace RewardPointsSystem.Application.DTOs.Admin
{
    /// <summary>
    /// Date range period for reports
    /// </summary>
    public class ReportPeriodDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    /// <summary>
    /// Points summary report DTO
    /// </summary>
    public class PointsReportDto
    {
        public ReportPeriodDto Period { get; set; } = new();
        public int TotalUsers { get; set; }
        public int TotalPointsDistributed { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public int TotalPointsInCirculation { get; set; }
        public double AverageBalance { get; set; }
        public IEnumerable<TopEarnerDto> TopEarners { get; set; } = new List<TopEarnerDto>();
    }

    /// <summary>
    /// Top earner info for reports
    /// </summary>
    public class TopEarnerDto
    {
        public Guid UserId { get; set; }
        public int TotalEarned { get; set; }
        public int CurrentBalance { get; set; }
    }

    /// <summary>
    /// User activity report DTO
    /// </summary>
    public class UsersReportDto
    {
        public ReportPeriodDto Period { get; set; } = new();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersInPeriod { get; set; }
    }

    /// <summary>
    /// Redemptions report DTO
    /// </summary>
    public class RedemptionsReportDto
    {
        public ReportPeriodDto Period { get; set; } = new();
        public int TotalRedemptions { get; set; }
        public int PendingRedemptions { get; set; }
        public int ApprovedRedemptions { get; set; }
        public int CancelledRedemptions { get; set; }
        public int TotalPointsSpent { get; set; }
    }

    /// <summary>
    /// Events report DTO
    /// </summary>
    public class EventsReportDto
    {
        public int Year { get; set; }
        public int TotalEvents { get; set; }
        public int CompletedEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalPointsDistributed { get; set; }
    }

    /// <summary>
    /// Points pool alert DTO
    /// </summary>
    public class PointsPoolAlertDto
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int TotalPointsPool { get; set; }
        public int RemainingPoints { get; set; }
        public double PercentageRemaining { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Admin count response DTO
    /// </summary>
    public class AdminCountDto
    {
        public int Count { get; set; }
    }
}
