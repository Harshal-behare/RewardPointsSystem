using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Points;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages points accounts and transactions.
    /// Simplified controller - generic exception handling moved to global exception handler.
    /// </summary>
    [Authorize]
    public class PointsController : BaseApiController
    {
        private readonly IPointsQueryService _pointsQueryService;
        private readonly IPointsAwardingService _awardingService;
        private readonly IPointsManagementService _pointsManagementService;
        private readonly ILogger<PointsController> _logger;

        public PointsController(
            IPointsQueryService pointsQueryService,
            IPointsAwardingService awardingService,
            IPointsManagementService pointsManagementService,
            ILogger<PointsController> logger)
        {
            _pointsQueryService = pointsQueryService;
            _awardingService = awardingService;
            _pointsManagementService = pointsManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Get user's points account
        /// </summary>
        [HttpGet("accounts/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<PointsAccountResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserPointsAccount(Guid userId)
        {
            var accountDto = await _pointsQueryService.GetUserPointsAccountAsync(userId);
            if (accountDto == null)
                return NotFoundError($"User with ID {userId} not found");

            return Success(accountDto);
        }

        /// <summary>
        /// Get user's transaction history (paginated)
        /// </summary>
        [HttpGet("transactions/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<TransactionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserTransactions(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (transactions, totalCount) = await _pointsQueryService.GetUserTransactionsAsync(userId, page, pageSize);
            var pagedResponse = PagedResponse<TransactionResponseDto>.Create(transactions, page, pageSize, totalCount);
            return PagedSuccess(pagedResponse);
        }

        /// <summary>
        /// Get all transactions (Admin only)
        /// </summary>
        [HttpGet("transactions")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<TransactionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var (transactions, totalCount) = await _pointsQueryService.GetAllTransactionsAsync(page, pageSize);
            var response = PagedSuccess(transactions, totalCount, page, pageSize);
            return Ok(response);
        }

        /// <summary>
        /// Award points to user (Admin only)
        /// </summary>
        [HttpPost("award")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AwardPoints([FromBody] AddPointsDto dto)
        {
            try
            {
                var adminId = GetCurrentUserId();
                await _awardingService.AwardPointsAsync(dto.UserId, dto.Points, dto.Description, dto.EventId, adminId);
                return Success<object>(null, $"Successfully awarded {dto.Points} points");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"User with ID {dto.UserId} not found");
            }
        }

        /// <summary>
        /// Deduct points from user (Admin only)
        /// </summary>
        [HttpPost("deduct")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeductPoints([FromBody] DeductPointsDto dto)
        {
            var adminUserId = GetCurrentUserId();
            if (!adminUserId.HasValue)
                return UnauthorizedError("Admin user not authenticated");

            var result = await _pointsManagementService.DeductPointsAsync(
                dto.UserId, dto.Points, dto.Reason, adminUserId.Value);

            if (!result.Success)
            {
                if (result.ErrorMessage!.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFoundError(result.ErrorMessage);

                return Error(result.ErrorMessage, 400);
            }

            return Success<object>(null, $"Successfully deducted {dto.Points} points");
        }

        /// <summary>
        /// Get points leaderboard
        /// </summary>
        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PointsAccountResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int top = 10)
        {
            var leaderboard = await _pointsQueryService.GetLeaderboardAsync(top);
            return Success(leaderboard);
        }

        /// <summary>
        /// Get points system summary (Admin only)
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PointsSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPointsSummary()
        {
            var summary = await _pointsQueryService.GetPointsSummaryAsync();
            return Success(summary);
        }
    }
}
