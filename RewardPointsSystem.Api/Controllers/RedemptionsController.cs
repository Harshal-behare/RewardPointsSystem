using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RewardPointsSystem.Application.DTOs.Common;
using RewardPointsSystem.Application.DTOs.Redemptions;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Api.Controllers
{
    /// <summary>
    /// Manages product redemption operations
    /// </summary>
    [Authorize]
    public class RedemptionsController : BaseApiController
    {
        private readonly IRedemptionOrchestrator _redemptionOrchestrator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RedemptionsController> _logger;

        public RedemptionsController(
            IRedemptionOrchestrator redemptionOrchestrator,
            IUnitOfWork unitOfWork,
            ILogger<RedemptionsController> logger)
        {
            _redemptionOrchestrator = redemptionOrchestrator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Get all redemptions (filtered by user for employees, all for admin)
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="status">Filter by status</param>
        /// <response code="200">Returns paginated redemptions</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<RedemptionResponseDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRedemptions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null)
        {
            try
            {
                // TODO: Check user role and filter by userId if not admin
                var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                
                // Apply pagination
                var totalCount = redemptions.Count();
                var pagedRedemptions = redemptions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RedemptionResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        ProductId = r.ProductId,
                        ProductName = r.Product?.Name,
                        PointsSpent = r.PointsSpent,
                        Status = r.Status.ToString(),
                        RequestedAt = r.RequestedAt,
                        ApprovedAt = r.ApprovedAt,
                        DeliveredAt = r.DeliveredAt
                    });

                var response = PagedSuccess(pagedRedemptions, totalCount, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving redemptions");
                return Error("Failed to retrieve redemptions");
            }
        }

        /// <summary>
        /// Get redemption by ID
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <response code="200">Returns redemption details</response>
        /// <response code="404">Redemption not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RedemptionDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRedemptionById(Guid id)
        {
            try
            {
                var redemption = await _unitOfWork.Redemptions.GetByIdAsync(id);
                if (redemption == null)
                    return NotFoundError($"Redemption with ID {id} not found");

                // TODO: Map to RedemptionDetailsDto with full details
                return Success(redemption);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving redemption {RedemptionId}", id);
                return Error("Failed to retrieve redemption");
            }
        }

        /// <summary>
        /// Create a new redemption request
        /// </summary>
        /// <param name="dto">Redemption creation data</param>
        /// <response code="201">Redemption created successfully</response>
        /// <response code="400">Insufficient points or product out of stock</response>
        /// <response code="404">User or product not found</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RedemptionResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRedemption([FromBody] CreateRedemptionDto dto)
        {
            try
            {
                var redemption = await _redemptionOrchestrator.CreateRedemptionAsync(
                    dto.UserId,
                    dto.ProductId,
                    dto.Quantity);

                var redemptionDto = new RedemptionResponseDto
                {
                    Id = redemption.Id,
                    UserId = redemption.UserId,
                    ProductId = redemption.ProductId,
                    ProductName = redemption.Product?.Name,
                    PointsSpent = redemption.PointsSpent,
                    Status = redemption.Status.ToString(),
                    RequestedAt = redemption.RequestedAt
                };

                return Created(redemptionDto, "Redemption request created successfully");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError("User or product not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating redemption");
                return Error("Failed to create redemption");
            }
        }

        /// <summary>
        /// Approve redemption (Admin only)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <param name="dto">Approval data</param>
        /// <response code="200">Redemption approved successfully</response>
        /// <response code="404">Redemption not found</response>
        [HttpPatch("{id}/approve")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveRedemption(Guid id, [FromBody] ApproveRedemptionDto dto)
        {
            try
            {
                await _redemptionOrchestrator.ApproveRedemptionAsync(id, dto.ApprovedBy);
                return Success<object>(null, "Redemption approved successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Redemption with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving redemption {RedemptionId}", id);
                return Error("Failed to approve redemption");
            }
        }

        /// <summary>
        /// Mark redemption as delivered (Admin only)
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <param name="dto">Delivery data</param>
        /// <response code="200">Redemption marked as delivered</response>
        /// <response code="404">Redemption not found</response>
        [HttpPatch("{id}/deliver")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsDelivered(Guid id, [FromBody] DeliverRedemptionDto dto)
        {
            try
            {
                await _redemptionOrchestrator.MarkAsDeliveredAsync(id);
                return Success<object>(null, "Redemption marked as delivered");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Redemption with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking redemption as delivered {RedemptionId}", id);
                return Error("Failed to mark as delivered");
            }
        }

        /// <summary>
        /// Cancel redemption
        /// </summary>
        /// <param name="id">Redemption ID</param>
        /// <param name="dto">Cancellation data</param>
        /// <response code="200">Redemption cancelled successfully</response>
        /// <response code="404">Redemption not found</response>
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelRedemption(Guid id, [FromBody] CancelRedemptionDto dto)
        {
            try
            {
                await _redemptionOrchestrator.CancelRedemptionAsync(id, dto.CancellationReason);
                return Success<object>(null, "Redemption cancelled and points refunded");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Redemption with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling redemption {RedemptionId}", id);
                return Error("Failed to cancel redemption");
            }
        }

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
            try
            {
                // TODO: Get userId from JWT claims
                var userId = Guid.Empty; // Placeholder
                
                var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                var userRedemptions = redemptions.Where(r => r.UserId == userId);
                
                var totalCount = userRedemptions.Count();
                var pagedRedemptions = userRedemptions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RedemptionResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        ProductId = r.ProductId,
                        ProductName = r.Product?.Name,
                        PointsSpent = r.PointsSpent,
                        Status = r.Status.ToString(),
                        RequestedAt = r.RequestedAt,
                        ApprovedAt = r.ApprovedAt,
                        DeliveredAt = r.DeliveredAt
                    });

                var response = PagedSuccess(pagedRedemptions, totalCount, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user redemptions");
                return Error("Failed to retrieve redemptions");
            }
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
            try
            {
                var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                var pendingRedemptions = redemptions
                    .Where(r => r.Status == RewardPointsSystem.Domain.Entities.Operations.RedemptionStatus.Pending)
                    .Select(r => new RedemptionResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        ProductId = r.ProductId,
                        ProductName = r.Product?.Name,
                        PointsSpent = r.PointsSpent,
                        Status = r.Status.ToString(),
                        RequestedAt = r.RequestedAt
                    });

                return Success(pendingRedemptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending redemptions");
                return Error("Failed to retrieve pending redemptions");
            }
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
            try
            {
                var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                
                var totalCount = redemptions.Count();
                var pagedRedemptions = redemptions
                    .OrderByDescending(r => r.RequestedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RedemptionResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        ProductId = r.ProductId,
                        ProductName = r.Product?.Name,
                        PointsSpent = r.PointsSpent,
                        Status = r.Status.ToString(),
                        RequestedAt = r.RequestedAt,
                        ApprovedAt = r.ApprovedAt,
                        DeliveredAt = r.DeliveredAt
                    });

                var response = PagedSuccess(pagedRedemptions, totalCount, page, pageSize);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving redemption history");
                return Error("Failed to retrieve redemption history");
            }
        }
    }
}
