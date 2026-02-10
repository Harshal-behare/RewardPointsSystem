using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs.Redemptions;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Redemptions;

/// <summary>
/// Application layer service for redemption management operations.
/// Uses ICurrentUserContext for authentication and Result pattern for error handling.
/// All authorization checks are performed in this layer.
/// </summary>
public class RedemptionManagementService : IRedemptionManagementService
{
    private readonly IRedemptionOrchestrator _redemptionOrchestrator;
    private readonly IRedemptionQueryService _redemptionQueryService;
    private readonly ICurrentUserContext _currentUserContext;

    public RedemptionManagementService(
        IRedemptionOrchestrator redemptionOrchestrator,
        IRedemptionQueryService redemptionQueryService,
        ICurrentUserContext currentUserContext)
    {
        _redemptionOrchestrator = redemptionOrchestrator;
        _redemptionQueryService = redemptionQueryService;
        _currentUserContext = currentUserContext;
    }

    public async Task<Result<RedemptionResponseDto>> CreateRedemptionAsync(CreateRedemptionDto dto)
    {
        // Get user from ICurrentUserContext instead of DTO
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
            return Result<RedemptionResponseDto>.Unauthorized("User not authenticated");

        try
        {
            var redemption = await _redemptionOrchestrator.CreateRedemptionAsync(
                userId.Value, dto.ProductId, dto.Quantity);

            var redemptionDto = new RedemptionResponseDto
            {
                Id = redemption.Id,
                UserId = redemption.UserId,
                ProductId = redemption.ProductId,
                ProductName = redemption.Product?.Name ?? "Unknown",
                PointsSpent = redemption.PointsSpent,
                Quantity = redemption.Quantity,
                Status = redemption.Status.ToString(),
                RequestedAt = redemption.RequestedAt,
                RejectionReason = redemption.RejectionReason ?? string.Empty
            };

            return Result<RedemptionResponseDto>.Success(redemptionDto);
        }
        catch (InvalidOperationException ex)
        {
            // Determine error type based on message
            if (ex.Message.ToLower().Contains("insufficient"))
                return Result<RedemptionResponseDto>.BusinessRuleViolation(ex.Message);
            if (ex.Message.ToLower().Contains("stock") || ex.Message.ToLower().Contains("available"))
                return Result<RedemptionResponseDto>.BusinessRuleViolation(ex.Message);
            return Result<RedemptionResponseDto>.ValidationFailure(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return Result<RedemptionResponseDto>.NotFound("Product not found");
        }
    }

    public async Task<Result<RedemptionDetailsDto>> GetRedemptionByIdAsync(Guid redemptionId)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
            return Result<RedemptionDetailsDto>.Unauthorized("User not authenticated");

        var redemption = await _redemptionQueryService.GetRedemptionByIdAsync(redemptionId);
        if (redemption == null)
            return Result<RedemptionDetailsDto>.NotFound($"Redemption with ID {redemptionId} not found");

        // Authorization check: users can only view their own redemptions unless admin
        var isAdmin = _currentUserContext.IsInRole("Admin");
        if (!isAdmin && redemption.UserId != userId.Value)
            return Result<RedemptionDetailsDto>.Forbidden("You do not have access to this redemption");

        return Result<RedemptionDetailsDto>.Success(redemption);
    }

    public async Task<Result> ApproveRedemptionAsync(Guid redemptionId)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
            return Result.Unauthorized("User not authenticated");

        if (!_currentUserContext.IsInRole("Admin"))
            return Result.Forbidden("Only administrators can approve redemptions");

        try
        {
            await _redemptionOrchestrator.ApproveRedemptionAsync(redemptionId, userId.Value);
            return Result.Success();
        }
        catch (KeyNotFoundException)
        {
            return Result.NotFound($"Redemption with ID {redemptionId} not found");
        }
        catch (InvalidOperationException ex)
        {
            return Result.BusinessRuleViolation(ex.Message);
        }
    }

    public async Task<Result> RejectRedemptionAsync(Guid redemptionId, string reason)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
            return Result.Unauthorized("User not authenticated");

        if (!_currentUserContext.IsInRole("Admin"))
            return Result.Forbidden("Only administrators can reject redemptions");

        try
        {
            await _redemptionOrchestrator.RejectRedemptionAsync(redemptionId, reason);
            return Result.Success();
        }
        catch (KeyNotFoundException)
        {
            return Result.NotFound($"Redemption with ID {redemptionId} not found");
        }
        catch (InvalidOperationException ex)
        {
            return Result.BusinessRuleViolation(ex.Message);
        }
    }

    public async Task<Result> MarkAsDeliveredAsync(Guid redemptionId)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
            return Result.Unauthorized("User not authenticated");

        if (!_currentUserContext.IsInRole("Admin"))
            return Result.Forbidden("Only administrators can mark redemptions as delivered");

        try
        {
            await _redemptionOrchestrator.DeliverRedemptionAsync(redemptionId, userId.Value);
            return Result.Success();
        }
        catch (KeyNotFoundException)
        {
            return Result.NotFound($"Redemption with ID {redemptionId} not found");
        }
        catch (InvalidOperationException ex)
        {
            return Result.BusinessRuleViolation(ex.Message);
        }
    }

    public async Task<Result> CancelRedemptionAsync(Guid redemptionId)
    {
        var userId = _currentUserContext.UserId;
        if (!userId.HasValue)
            return Result.Unauthorized("User not authenticated");

        // Get redemption to check ownership
        var redemption = await _redemptionQueryService.GetRedemptionByIdAsync(redemptionId);
        if (redemption == null)
            return Result.NotFound($"Redemption with ID {redemptionId} not found");

        // Authorization: users can cancel their own, admins can cancel any
        var isAdmin = _currentUserContext.IsInRole("Admin");
        if (!isAdmin && redemption.UserId != userId.Value)
            return Result.Forbidden("You can only cancel your own redemptions");

        try
        {
            await _redemptionOrchestrator.CancelRedemptionAsync(redemptionId, "Cancelled by user");
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.BusinessRuleViolation(ex.Message);
        }
    }
}
