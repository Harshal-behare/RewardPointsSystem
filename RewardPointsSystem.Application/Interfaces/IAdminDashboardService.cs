using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Events;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IAdminDashboardService
    /// Responsibility: Provide admin queries only (read-only aggregations)
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IAdminDashboardService
    {
        Task<DashboardStats> GetDashboardStatsAsync();
        Task<IEnumerable<Event>> GetEventsNeedingAllocationAsync();
        Task<IEnumerable<Redemption>> GetPendingRedemptionsAsync();
        Task<IEnumerable<InventoryAlert>> GetInventoryAlertsAsync();
        Task<PointsSummary> GetPointsSummaryAsync();
    }
}