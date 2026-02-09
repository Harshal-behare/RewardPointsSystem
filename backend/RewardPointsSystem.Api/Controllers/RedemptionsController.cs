using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Redemptions;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages product redemption operations.
    /// Clean Architecture compliant - delegates ALL business logic to Application layer services.
    /// No business rules, authorization checks, or user context handling in this controller.
    /// </summary>
    [Authorize]
    public class RedemptionsController : BaseApiController
    {
        private readonly IRedemptionManagementService _redemptionManagementService;
        private readonly IRedemptionQueryService _redemptionQueryService;
        private readonly ILogger<RedemptionsController> _logger;

        public RedemptionsController(
            IRedemptionManagementService redemptionManagementService,
            IRedemptionQueryService redemptionQueryService,
            ILogger<RedemptionsController> logger)
        {
            _redemptionManagementService = redemptionManagementService;
            _redemptionQueryService = redemptionQueryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all redemptions (Admin only - returns all redemptions in the system)
        /// </summary>
        /// <response code="200">Returns all redemptions</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RedemptionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRedemptions()
        {
            var redemptions = await _redemptionQueryService.GetAllRedemptionsAsync();
            return Success(redemptions);
        }

        /// <summary>
        /// Get redemption by ID (ownership check in Application layer)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <response code="200">Returns redemption details</response>
        /// <response code="404">Redemption not found</response>
        /// <response code="403">Access denied to this redemption</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RedemptionDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRedemptionById(Guid id)
        {
            var result = await _redemptionManagementService.GetRedemptionByIdAsync(id);
            return ToActionResult(result);
        }

        /// <summary>
        /// Create a new redemption request.
        /// UserId is derived from JWT - not included in DTO.
        /// </summary>
        /// <param name="dto">Redemption creation data (ProductId, Quantity only)</param>
        /// <response code="201">Redemption created successfully</response>
        /// <response code="400">Insufficient points or product out of stock</response>
        /// <response code="404">Product not found</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RedemptionResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRedemption([FromBody] CreateRedemptionDto dto)
        {
            var result = await _redemptionManagementService.CreateRedemptionAsync(dto);
            return ToCreatedResult(result, "Redemption request created successfully");
        }

        /// <summary>
        /// Approve redemption (Admin only - authorization in Application layer)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <response code="200">Redemption approved successfully</response>
        /// <response code="404">Redemption not found</response>
        /// <response code="403">Not authorized to approve</response>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ApproveRedemption(Guid id)
        {
            var result = await _redemptionManagementService.ApproveRedemptionAsync(id);
            return ToActionResult(result, "Redemption approved successfully");
        }

        /// <summary>
        /// Mark redemption as delivered (Admin only)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <response code="200">Redemption marked as delivered successfully</response>
        /// <response code="404">Redemption not found</response>
        [HttpPatch("{id}/deliver")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeliverRedemption(Guid id)
        {
            var result = await _redemptionManagementService.MarkAsDeliveredAsync(id);
            return ToActionResult(result, "Redemption delivered successfully");
        }

        /// <summary>
        /// Reject redemption (Admin only)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <param name="dto">Rejection data with reason</param>
        /// <response code="200">Redemption rejected successfully</response>
        /// <response code="404">Redemption not found</response>
        [HttpPatch("{id}/reject")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectRedemption(Guid id, [FromBody] RejectRedemptionDto dto)
        {
            var result = await _redemptionManagementService.RejectRedemptionAsync(id, dto.RejectionReason);
            return ToActionResult(result, "Redemption rejected successfully");
        }

        /// <summary>
        /// Cancel redemption (owner or Admin - authorization in Application layer)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <response code="200">Redemption cancelled successfully</response>
        /// <response code="404">Redemption not found</response>
        /// <response code="403">Not authorized to cancel</response>
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelRedemption(Guid id)
        {
            var result = await _redemptionManagementService.CancelRedemptionAsync(id);
            return ToActionResult(result, "Redemption cancelled and points refunded");
        }

        // ============================================
        // Query endpoints (read-only, minimal business logic)
        // These remain simple as they use query services directly
        // ============================================

        /// <summary>
        /// Get current user's redemptions
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <response code="200">Returns user's redemptions</response>
        [HttpGet("my-redemptions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<RedemptionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRedemptions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Note: In a full refactor, this would also use ICurrentUserContext via a query service
            // For now, keeping minimal query logic here
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return UnauthorizedError("User not authenticated");

            var (redemptions, totalCount) = await _redemptionQueryService.GetUserRedemptionsPagedAsync(userId, page, pageSize);
            var pagedResponse = PagedResponse<RedemptionResponseDto>.Create(redemptions, page, pageSize, totalCount);
            return PagedSuccess(pagedResponse);
        }

        /// <summary>
        /// Get pending redemptions (Admin only)
        /// </summary>
        /// <response code="200">Returns pending redemptions</response>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<RedemptionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingRedemptions()
        {
            var pendingRedemptions = await _redemptionQueryService.GetPendingRedemptionsAsync();
            return Success(pendingRedemptions);
        }

        /// <summary>
        /// Get pending redemptions count for a specific user (Admin only)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <response code="200">Returns count of pending redemptions</response>
        [HttpGet("user/{userId}/pending-count")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PendingCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserPendingRedemptionsCount(Guid userId)
        {
            var count = await _redemptionQueryService.GetUserPendingRedemptionsCountAsync(userId);
            return Success(new PendingCountDto { Count = count });
        }

        /// <summary>
        /// Get pending redemptions count for a specific product (Admin only)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <response code="200">Returns count of pending redemptions</response>
        [HttpGet("product/{productId}/pending-count")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PendingCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductPendingRedemptionsCount(Guid productId)
        {
            var count = await _redemptionQueryService.GetProductPendingRedemptionsCountAsync(productId);
            return Success(new PendingCountDto { Count = count });
        }

        /// <summary>
        /// Get redemption history (Admin only)
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <response code="200">Returns redemption history</response>
        [HttpGet("history")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<RedemptionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRedemptionHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var (redemptions, totalCount) = await _redemptionQueryService.GetRedemptionHistoryPagedAsync(page, pageSize);
            var pagedResponse = PagedResponse<RedemptionResponseDto>.Create(redemptions, page, pageSize, totalCount);
            return PagedSuccess(pagedResponse);
        }
    }
}
