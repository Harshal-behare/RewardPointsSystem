using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.Interfaces;
using System.Security.Claims;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Admin dashboard and reporting endpoints
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseApiController
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly IUserService _userService;
        private readonly IUserPointsAccountService _accountService;
        private readonly IEventService _eventService;
        private readonly IInventoryService _inventoryService;
        private readonly IAdminBudgetService _budgetService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminDashboardService dashboardService,
            IUserService userService,
            IUserPointsAccountService accountService,
            IEventService eventService,
            IInventoryService inventoryService,
            IAdminBudgetService budgetService,
            IUnitOfWork unitOfWork,
            ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _userService = userService;
            _accountService = accountService;
            _eventService = eventService;
            _inventoryService = inventoryService;
            _budgetService = budgetService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard statistics
        /// </summary>
        /// <response code="200">Returns dashboard statistics</response>
        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                return Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return Error("Failed to retrieve dashboard statistics");
            }
        }

        /// <summary>
        /// Get points summary report
        /// </summary>
        /// <param name="startDate">Start date for report</param>
        /// <param name="endDate">End date for report</param>
        /// <response code="200">Returns points summary</response>
        [HttpGet("reports/points")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointsReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var accounts = await _accountService.GetAllAccountsAsync();
                
                var report = new
                {
                    Period = new { Start = startDate, End = endDate },
                    TotalUsers = accounts.Count(),
                    TotalPointsDistributed = accounts.Sum(a => a.TotalEarned),
                    TotalPointsRedeemed = accounts.Sum(a => a.TotalRedeemed),
                    TotalPointsInCirculation = accounts.Sum(a => a.CurrentBalance),
                    AverageBalance = accounts.Average(a => a.CurrentBalance),
                    TopEarners = accounts.OrderByDescending(a => a.TotalEarned).Take(10)
                };

                return Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating points report");
                return Error("Failed to generate points report");
            }
        }

        /// <summary>
        /// Get user activity report
        /// </summary>
        /// <param name="startDate">Start date for report</param>
        /// <param name="endDate">End date for report</param>
        /// <response code="200">Returns user activity statistics</response>
        [HttpGet("reports/users")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var users = await _userService.GetActiveUsersAsync();
                
                var report = new
                {
                    Period = new { Start = startDate, End = endDate },
                    TotalUsers = users.Count(),
                    ActiveUsers = users.Count(u => u.IsActive),
                    NewUsersInPeriod = users.Count(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                };

                return Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating users report");
                return Error("Failed to generate users report");
            }
        }

        /// <summary>
        /// Get redemptions report
        /// </summary>
        /// <param name="startDate">Start date for report</param>
        /// <param name="endDate">End date for report</param>
        /// <response code="200">Returns redemption statistics</response>
        [HttpGet("reports/redemptions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRedemptionsReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                var periodRedemptions = redemptions.Where(r => 
                    r.RequestedAt >= startDate && r.RequestedAt <= endDate);
                
                var report = new
                {
                    Period = new { Start = startDate, End = endDate },
                    TotalRedemptions = periodRedemptions.Count(),
                    PendingRedemptions = periodRedemptions.Count(r => 
                        r.Status == RewardPointsSystem.Domain.Entities.Operations.RedemptionStatus.Pending),
                    ApprovedRedemptions = periodRedemptions.Count(r => 
                        r.Status == RewardPointsSystem.Domain.Entities.Operations.RedemptionStatus.Approved),
                    CancelledRedemptions = periodRedemptions.Count(r => 
                        r.Status == RewardPointsSystem.Domain.Entities.Operations.RedemptionStatus.Cancelled),
                    TotalPointsSpent = periodRedemptions.Sum(r => r.PointsSpent)
                };

                return Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating redemptions report");
                return Error("Failed to generate redemptions report");
            }
        }

        /// <summary>
        /// Get events report
        /// </summary>
        /// <param name="year">Year for report</param>
        /// <response code="200">Returns event statistics</response>
        [HttpGet("reports/events")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventsReport([FromQuery] int? year = null)
        {
            try
            {
                year ??= DateTime.UtcNow.Year;

                var events = await _eventService.GetAllEventsAsync();
                var yearEvents = events.Where(e => e.EventDate.Year == year);
                
                var report = new
                {
                    Year = year,
                    TotalEvents = yearEvents.Count(),
                    CompletedEvents = yearEvents.Count(e => 
                        e.Status == RewardPointsSystem.Domain.Entities.Events.EventStatus.Completed),
                    UpcomingEvents = yearEvents.Count(e => 
                        e.Status == RewardPointsSystem.Domain.Entities.Events.EventStatus.Upcoming),
                    TotalParticipants = yearEvents.Sum(e => e.Participants.Count),
                    TotalPointsDistributed = yearEvents.Sum(e => 
                        e.TotalPointsPool - e.GetAvailablePointsPool())
                };

                return Success(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating events report");
                return Error("Failed to generate events report");
            }
        }

        /// <summary>
        /// Get low stock alerts
        /// </summary>
        /// <response code="200">Returns products with low stock</response>
        [HttpGet("alerts/inventory")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInventoryAlerts()
        {
            try
            {
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                
                var alerts = lowStockItems.Select(item => new
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    CurrentStock = item.CurrentStock,
                    ReorderLevel = item.ReorderLevel,
                    AlertType = item.AlertType
                });

                return Success(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory alerts");
                return Error("Failed to retrieve inventory alerts");
            }
        }

        /// <summary>
        /// Get points pool alerts for events
        /// </summary>
        /// <response code="200">Returns events with low points pool</response>
        [HttpGet("alerts/points")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointsPoolAlerts()
        {
            try
            {
                var events = await _eventService.GetUpcomingEventsAsync();
                
                var alerts = events
                    .Where(e => e.GetAvailablePointsPool() < e.TotalPointsPool * 0.2) // Less than 20% remaining
                    .Select(e => new
                    {
                        EventId = e.Id,
                        EventName = e.Name,
                        EventDate = e.EventDate,
                        TotalPointsPool = e.TotalPointsPool,
                        RemainingPoints = e.GetAvailablePointsPool(),
                        PercentageRemaining = (double)e.GetAvailablePointsPool() / e.TotalPointsPool * 100,
                        Status = e.GetAvailablePointsPool() == 0 ? "Depleted" : "Low"
                    });

                return Success(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving points pool alerts");
                return Error("Failed to retrieve points pool alerts");
            }
        }

        /// <summary>
        /// Get count of active admin users
        /// </summary>
        /// <response code="200">Returns admin count</response>
        [HttpGet("admin-count")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminCount()
        {
            try
            {
                // Get admin role
                var adminRole = await _unitOfWork.Roles.GetAllAsync();
                var admin = adminRole.FirstOrDefault(r => r.Name.ToLower() == "admin");

                if (admin == null)
                {
                    return Success(new { count = 0 });
                }

                // Get all user roles with admin role
                var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
                var adminUserIds = userRoles.Where(ur => ur.RoleId == admin.Id).Select(ur => ur.UserId).ToList();

                // Get active admin users
                var users = await _userService.GetActiveUsersAsync();
                var activeAdminCount = users.Count(u => adminUserIds.Contains(u.Id) && u.IsActive);

                return Success(new { count = activeAdminCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin count");
                return Error("Failed to retrieve admin count");
            }
        }

        /// <summary>
        /// Get current admin's monthly budget status
        /// </summary>
        /// <response code="200">Returns budget status or null if no budget set</response>
        [HttpGet("budget")]
        [ProducesResponseType(typeof(ApiResponse<AdminBudgetResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBudget()
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                if (adminUserId == null)
                    return Unauthorized();

                var budget = await _budgetService.GetCurrentBudgetAsync(adminUserId.Value);
                return Success(budget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin budget");
                return Error("Failed to retrieve budget");
            }
        }

        /// <summary>
        /// Set or update monthly budget limit
        /// </summary>
        /// <param name="dto">Budget configuration</param>
        /// <response code="200">Budget updated successfully</response>
        /// <response code="422">Validation failed</response>
        [HttpPut("budget")]
        [ProducesResponseType(typeof(ApiResponse<AdminBudgetResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> SetBudget([FromBody] SetBudgetDto dto)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                if (adminUserId == null)
                    return Unauthorized();

                var budget = await _budgetService.SetBudgetAsync(adminUserId.Value, dto);
                return Success(budget, "Budget updated successfully");
            }
            catch (InvalidOperationException ex)
            {
                // Budget limit validation error (e.g., limit less than already awarded after 10th)
                return Error(ex.Message, 400);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, 422);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting admin budget");
                return Error("Failed to set budget");
            }
        }

        /// <summary>
        /// Validate if admin can award specified points within budget
        /// </summary>
        /// <param name="points">Points to validate</param>
        /// <response code="200">Returns validation result</response>
        [HttpGet("budget/validate")]
        [ProducesResponseType(typeof(ApiResponse<BudgetValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateBudget([FromQuery] int points)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                if (adminUserId == null)
                    return Unauthorized();

                var result = await _budgetService.ValidatePointsAwardAsync(adminUserId.Value, points);
                return Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating budget for {Points} points", points);
                return Error("Failed to validate budget");
            }
        }

        /// <summary>
        /// Get budget usage history for last 12 months
        /// </summary>
        /// <param name="months">Number of months to retrieve (default 12)</param>
        /// <response code="200">Returns budget history</response>
        [HttpGet("budget/history")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BudgetHistoryItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBudgetHistory([FromQuery] int months = 12)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                if (adminUserId == null)
                    return Unauthorized();

                var history = await _budgetService.GetBudgetHistoryAsync(adminUserId.Value, months);
                return Success(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget history");
                return Error("Failed to retrieve budget history");
            }
        }
    }
}
