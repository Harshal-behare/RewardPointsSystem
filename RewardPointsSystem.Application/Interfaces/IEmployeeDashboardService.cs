using System;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IEmployeeDashboardService
    /// Responsibility: Provide employee-specific dashboard queries (read-only)
    /// </summary>
    public interface IEmployeeDashboardService
    {
        Task<EmployeeDashboardDto> GetDashboardAsync(Guid userId);
    }
}
