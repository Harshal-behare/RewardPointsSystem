using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Admin dashboard and reporting endpoints.
    /// Simplified controller - generic exception handling moved to global exception handler.
    /// All data projections moved to Application layer services.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IAdminReportService _reportService;
        private readonly IAdminAlertService _alertService;
        private readonly IInventoryService _inventoryService;
        private readonly IAdminBudgetService _budgetService;

        public AdminController(
            IAdminDashboardService dashboardService,
            IAdminReportService reportService,
            IAdminAlertService alertService,
            IInventoryService inventoryService,
            IAdminBudgetService budgetService)
        {
            _dashboardService = dashboardService;
            _reportService = reportService;
            _alertService = alertService;
            _inventoryService = inventoryService;
            _budgetService = budgetService;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Success(stats);
        }

        /// <summary>
        /// Get points summary report
        /// </summary>
        [HttpGet("reports/points")]
        [ProducesResponseType(typeof(ApiResponse<PointsReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointsReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var report = await _reportService.GetPointsReportAsync(startDate, endDate);
            return Success(report);
        }

        /// <summary>
        /// Get user activity report
        /// </summary>
        [HttpGet("reports/users")]
        [ProducesResponseType(typeof(ApiResponse<UsersReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var report = await _reportService.GetUsersReportAsync(startDate, endDate);
            return Success(report);
        }

        /// <summary>
        /// Get redemptions report
        /// </summary>
        [HttpGet("reports/redemptions")]
        [ProducesResponseType(typeof(ApiResponse<RedemptionsReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRedemptionsReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var report = await _reportService.GetRedemptionsReportAsync(startDate, endDate);
            return Success(report);
        }

        /// <summary>
        /// Get events report
        /// </summary>
        [HttpGet("reports/events")]
        [ProducesResponseType(typeof(ApiResponse<EventsReportDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventsReport([FromQuery] int? year = null)
        {
            var report = await _reportService.GetEventsReportAsync(year);
            return Success(report);
        }

        /// <summary>
        /// Get low stock alerts
        /// </summary>
        [HttpGet("alerts/inventory")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInventoryAlerts()
        {
            var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
            return Success(lowStockItems);
        }

        /// <summary>
        /// Get points pool alerts for events
        /// </summary>
        [HttpGet("alerts/points")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PointsPoolAlertDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointsPoolAlerts()
        {
            var alerts = await _alertService.GetPointsPoolAlertsAsync();
            return Success(alerts);
        }

        /// <summary>
        /// Get count of active admin users
        /// </summary>
        [HttpGet("admin-count")]
        [ProducesResponseType(typeof(ApiResponse<AdminCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminCount()
        {
            var result = await _reportService.GetAdminCountAsync();
            return Success(result);
        }

        /// <summary>
        /// Get current admin's monthly budget status
        /// </summary>
        [HttpGet("budget")]
        [ProducesResponseType(typeof(ApiResponse<AdminBudgetResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBudget()
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null)
                return Unauthorized();

            var budget = await _budgetService.GetCurrentBudgetAsync(adminUserId.Value);
            return Success(budget);
        }

        /// <summary>
        /// Set or update monthly budget limit
        /// </summary>
        [HttpPut("budget")]
        [ProducesResponseType(typeof(ApiResponse<AdminBudgetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> SetBudget([FromBody] SetBudgetDto dto)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null)
                return Unauthorized();

            try
            {
                var budget = await _budgetService.SetBudgetAsync(adminUserId.Value, dto);
                return Success(budget, "Budget updated successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 422);
            }
        }

        /// <summary>
        /// Validate if admin can award specified points within budget
        /// </summary>
        [HttpGet("budget/validate")]
        [ProducesResponseType(typeof(ApiResponse<BudgetValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateBudget([FromQuery] int points)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null)
                return Unauthorized();

            var result = await _budgetService.ValidatePointsAwardAsync(adminUserId.Value, points);
            return Success(result);
        }

        /// <summary>
        /// Get budget usage history for last 12 months
        /// </summary>
        [HttpGet("budget/history")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BudgetHistoryItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBudgetHistory([FromQuery] int months = 12)
        {
            var adminUserId = GetCurrentUserId();
            if (adminUserId == null)
                return Unauthorized();

            var history = await _budgetService.GetBudgetHistoryAsync(adminUserId.Value, months);
            return Success(history);
        }
    }
}
