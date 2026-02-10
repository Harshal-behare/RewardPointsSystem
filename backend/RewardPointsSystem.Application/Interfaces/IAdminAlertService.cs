using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Admin;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IAdminAlertService
    /// Responsibility: Provide admin alerts and notifications
    /// Clean Architecture - Encapsulates alert business logic
    /// </summary>
    public interface IAdminAlertService
    {
        /// <summary>
        /// Get events with low or depleted points pools
        /// </summary>
        Task<IEnumerable<PointsPoolAlertDto>> GetPointsPoolAlertsAsync();
    }
}
