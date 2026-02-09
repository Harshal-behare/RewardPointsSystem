using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;
using System.Security.Claims;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Employee dashboard and personal data endpoints
    /// </summary>
    [Authorize]
    public class EmployeeController : BaseApiController
    {
        private readonly IEmployeeDashboardService _dashboardService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeDashboardService dashboardService,
            ILogger<EmployeeController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get employee dashboard data
        /// Returns points summary, redemptions, events, and featured products
        /// </summary>
        /// <response code="200">Returns employee dashboard data</response>
        /// <response code="401">Not authenticated</response>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return UnauthorizedError("Invalid user credentials");
                }

                var dashboard = await _dashboardService.GetDashboardAsync(userId);
                return Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employee dashboard");
                return Error("Failed to retrieve dashboard data");
            }
        }
    }
}
