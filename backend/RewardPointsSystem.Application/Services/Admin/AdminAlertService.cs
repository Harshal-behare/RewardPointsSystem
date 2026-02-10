using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Admin
{
    /// <summary>
    /// Service: AdminAlertService
    /// Responsibility: Provide admin alerts for monitoring
    /// Clean Architecture - Application layer encapsulates alert business logic
    /// </summary>
    public class AdminAlertService : IAdminAlertService
    {
        private readonly IEventService _eventService;
        private const double LowPointsThreshold = 0.2; // 20% remaining

        public AdminAlertService(IEventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IEnumerable<PointsPoolAlertDto>> GetPointsPoolAlertsAsync()
        {
            var events = await _eventService.GetUpcomingEventsAsync();

            return events
                .Where(e => e.GetAvailablePointsPool() < e.TotalPointsPool * LowPointsThreshold)
                .Select(e => new PointsPoolAlertDto
                {
                    EventId = e.Id,
                    EventName = e.Name,
                    EventDate = e.EventDate,
                    TotalPointsPool = e.TotalPointsPool,
                    RemainingPoints = e.GetAvailablePointsPool(),
                    PercentageRemaining = e.TotalPointsPool > 0 
                        ? (double)e.GetAvailablePointsPool() / e.TotalPointsPool * 100 
                        : 0,
                    Status = e.GetAvailablePointsPool() == 0 ? "Depleted" : "Low"
                });
        }
    }
}
