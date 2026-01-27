using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        private readonly IUserService _userService;
        private readonly ILogger<RedemptionsController> _logger;

        public RedemptionsController(
            IRedemptionOrchestrator redemptionOrchestrator,
            IUnitOfWork unitOfWork,
            IUserService userService,
            ILogger<RedemptionsController> logger)
        {
            _redemptionOrchestrator = redemptionOrchestrator;
            _unitOfWork = unitOfWork;
            _userService = userService;
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
                // Get current user context
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                IEnumerable<RewardPointsSystem.Domain.Entities.Operations.Redemption> redemptions;
                
                if (isAdmin)
                {
                    // Admins can see all redemptions
                    redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                }
                else
                {
                    // Non-admins can only see their own redemptions
                    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                        return UnauthorizedError("User not authenticated");
                    
                    var allRedemptions = await _unitOfWork.Redemptions.GetAllAsync();
                    redemptions = allRedemptions.Where(r => r.UserId == currentUserId);
                }

                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<RewardPointsSystem.Domain.Entities.Operations.RedemptionStatus>(status, true, out var statusEnum))
                {
                    redemptions = redemptions.Where(r => r.Status == statusEnum);
                }
                
                // Get all users and products for lookup
                var allUsers = await _unitOfWork.Users.GetAllAsync();
                var allProducts = await _unitOfWork.Products.GetAllAsync();
                
                // Apply pagination
                var totalCount = redemptions.Count();
                var pagedRedemptions = redemptions
                    .OrderByDescending(r => r.RequestedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => {
                        var user = allUsers.FirstOrDefault(u => u.Id == r.UserId);
                        var product = allProducts.FirstOrDefault(p => p.Id == r.ProductId);
                        return new RedemptionResponseDto
                        {
                            Id = r.Id,
                            UserId = r.UserId,
                            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                            UserEmail = user?.Email ?? "",
                            ProductId = r.ProductId,
                            ProductName = product?.Name ?? r.Product?.Name ?? "Unknown Product",
                            PointsSpent = r.PointsSpent,
                            Quantity = r.Quantity,
                            Status = r.Status.ToString(),
                            RequestedAt = r.RequestedAt,
                            ApprovedAt = r.ApprovedAt,
                            RejectionReason = r.RejectionReason
                        };
                    });

                var pagedResponse = PagedResponse<RedemptionResponseDto>.Create(pagedRedemptions, page, pageSize, totalCount);
                return PagedSuccess(pagedResponse);
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
        /// <response code="403">Access denied to this redemption</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RedemptionDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRedemptionById(Guid id)
        {
            try
            {
                var redemption = await _unitOfWork.Redemptions.GetByIdAsync(id);
                if (redemption == null)
                    return NotFoundError($"Redemption with ID {id} not found");

                // Check authorization - non-admins can only view their own redemptions
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin)
                {
                    if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                        return UnauthorizedError("User not authenticated");
                    
                    if (redemption.UserId != currentUserId)
                        return ForbiddenError("You do not have access to this redemption");
                }

                // Get user details for the redemption
                var user = await _userService.GetUserByIdAsync(redemption.UserId);
                
                // Get approver details if approved
                string approverName = null;
                if (redemption.ApprovedBy.HasValue)
                {
                    var approver = await _userService.GetUserByIdAsync(redemption.ApprovedBy.Value);
                    approverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null;
                }

                var redemptionDto = new RedemptionDetailsDto
                {
                    Id = redemption.Id,
                    UserId = redemption.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : null,
                    UserEmail = user?.Email,
                    ProductId = redemption.ProductId,
                    ProductName = redemption.Product?.Name,
                    ProductCategory = redemption.Product?.ProductCategory?.Name,
                    PointsSpent = redemption.PointsSpent,
                    Status = redemption.Status.ToString(),
                    RequestedAt = redemption.RequestedAt,
                    ApprovedAt = redemption.ApprovedAt,
                    ApprovedBy = redemption.ApprovedBy,
                    ApprovedByName = approverName,
                    ProcessedAt = redemption.ProcessedAt,
                    RejectionReason = redemption.RejectionReason
                };

                return Success(redemptionDto);
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
                    Quantity = redemption.Quantity,
                    Status = redemption.Status.ToString(),
                    RequestedAt = redemption.RequestedAt,
                    RejectionReason = redemption.RejectionReason
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
                // Get admin user ID from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var adminUserId))
                    return UnauthorizedError("Admin user not authenticated");

                // Use the admin user ID from claims, overriding what was sent in the request
                await _redemptionOrchestrator.ApproveRedemptionAsync(id, adminUserId);
                return Success<object>(null, "Redemption approved successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Redemption with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving redemption {RedemptionId}", id);
                return Error("Failed to approve redemption");
            }
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
            try
            {
                await _redemptionOrchestrator.RejectRedemptionAsync(id, dto.RejectionReason);
                return Success<object>(null, "Redemption rejected successfully");
            }
            catch (KeyNotFoundException)
            {
                return NotFoundError($"Redemption with ID {id} not found");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting redemption {RedemptionId}", id);
                return Error("Failed to reject redemption");
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
        /// <response code="401">User not authenticated</response>
        [HttpGet("my-redemptions")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<RedemptionResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyRedemptions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Get userId from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return UnauthorizedError("User not authenticated");
                
                var redemptions = await _unitOfWork.Redemptions.GetAllAsync();
                var userRedemptions = redemptions.Where(r => r.UserId == userId);
                
                // Get all products for lookup
                var allProducts = await _unitOfWork.Products.GetAllAsync();
                
                var totalCount = userRedemptions.Count();
                var pagedRedemptions = userRedemptions
                    .OrderByDescending(r => r.RequestedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => {
                        var product = allProducts.FirstOrDefault(p => p.Id == r.ProductId);
                        return new RedemptionResponseDto
                        {
                            Id = r.Id,
                            UserId = r.UserId,
                            ProductId = r.ProductId,
                            ProductName = product?.Name ?? "Unknown Product",
                            PointsSpent = r.PointsSpent,
                            Quantity = r.Quantity,
                            Status = r.Status.ToString(),
                            RequestedAt = r.RequestedAt,
                            ApprovedAt = r.ApprovedAt,
                            RejectionReason = r.RejectionReason
                        };
                    });

                var pagedResponse = PagedResponse<RedemptionResponseDto>.Create(pagedRedemptions, page, pageSize, totalCount);
                return PagedSuccess(pagedResponse);
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
                
                // Get all users and products for lookup
                var allUsers = await _unitOfWork.Users.GetAllAsync();
                var allProducts = await _unitOfWork.Products.GetAllAsync();
                
                var pendingRedemptions = redemptions
                    .Where(r => r.Status == RewardPointsSystem.Domain.Entities.Operations.RedemptionStatus.Pending)
                    .Select(r => {
                        var user = allUsers.FirstOrDefault(u => u.Id == r.UserId);
                        var product = allProducts.FirstOrDefault(p => p.Id == r.ProductId);
                        return new RedemptionResponseDto
                        {
                            Id = r.Id,
                            UserId = r.UserId,
                            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                            UserEmail = user?.Email ?? "",
                            ProductId = r.ProductId,
                            ProductName = product?.Name ?? "Unknown Product",
                            PointsSpent = r.PointsSpent,
                            Quantity = r.Quantity,
                            Status = r.Status.ToString(),
                            RequestedAt = r.RequestedAt,
                            RejectionReason = r.RejectionReason
                        };
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
                
                // Get all users and products for lookup
                var allUsers = await _unitOfWork.Users.GetAllAsync();
                var allProducts = await _unitOfWork.Products.GetAllAsync();
                
                var totalCount = redemptions.Count();
                var pagedRedemptions = redemptions
                    .OrderByDescending(r => r.RequestedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => {
                        var user = allUsers.FirstOrDefault(u => u.Id == r.UserId);
                        var product = allProducts.FirstOrDefault(p => p.Id == r.ProductId);
                        return new RedemptionResponseDto
                        {
                            Id = r.Id,
                            UserId = r.UserId,
                            UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                            UserEmail = user?.Email ?? "",
                            ProductId = r.ProductId,
                            ProductName = product?.Name ?? "Unknown Product",
                            PointsSpent = r.PointsSpent,
                            Quantity = r.Quantity,
                            Status = r.Status.ToString(),
                            RequestedAt = r.RequestedAt,
                            ApprovedAt = r.ApprovedAt,
                            RejectionReason = r.RejectionReason
                        };
                    });

                var pagedResponse = PagedResponse<RedemptionResponseDto>.Create(pagedRedemptions, page, pageSize, totalCount);
                return PagedSuccess(pagedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving redemption history");
                return Error("Failed to retrieve redemption history");
            }
        }
    }
}
