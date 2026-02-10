using System;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Admin;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IAdminReportService
    /// Responsibility: Generate admin reports with proper aggregations
    /// Clean Architecture - Encapsulates reporting business logic
    /// </summary>
    public interface IAdminReportService
    {
        /// <summary>
        /// Generate points summary report for a date range
        /// </summary>
        Task<PointsReportDto> GetPointsReportAsync(DateTime? startDate, DateTime? endDate);
        
        /// <summary>
        /// Generate user activity report for a date range
        /// </summary>
        Task<UsersReportDto> GetUsersReportAsync(DateTime? startDate, DateTime? endDate);
        
        /// <summary>
        /// Generate redemptions report for a date range
        /// </summary>
        Task<RedemptionsReportDto> GetRedemptionsReportAsync(DateTime? startDate, DateTime? endDate);
        
        /// <summary>
        /// Generate events report for a specific year
        /// </summary>
        Task<EventsReportDto> GetEventsReportAsync(int? year);
        
        /// <summary>
        /// Get count of active admin users
        /// </summary>
        Task<AdminCountDto> GetAdminCountAsync();
    }
}
