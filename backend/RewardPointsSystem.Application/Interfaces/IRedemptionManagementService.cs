using RewardPointsSystem.Application.Common;
using RewardPointsSystem.Application.DTOs.Redemptions;

namespace RewardPointsSystem.Application.Interfaces;

/// <summary>
/// Application layer service for redemption management operations.
/// Handles redemption creation using ICurrentUserContext for authentication.
/// Uses unified Result pattern for error handling.
/// </summary>
public interface IRedemptionManagementService
{
    /// <summary>
    /// Creates a new redemption for the current user (derived from ICurrentUserContext).
    /// </summary>
    /// <param name="dto">Redemption creation DTO (does not include UserId)</param>
    /// <returns>Result with redemption data or error</returns>
    Task<Result<RedemptionResponseDto>> CreateRedemptionAsync(CreateRedemptionDto dto);

    /// <summary>
    /// Gets a redemption by ID, verifying the current user has access.
    /// </summary>
    /// <param name="redemptionId">Redemption ID</param>
    /// <returns>Result with redemption details or error</returns>
    Task<Result<RedemptionDetailsDto>> GetRedemptionByIdAsync(Guid redemptionId);

    /// <summary>
    /// Approves a redemption (Admin only).
    /// </summary>
    /// <param name="redemptionId">Redemption ID</param>
    /// <returns>Result indicating success or error</returns>
    Task<Result> ApproveRedemptionAsync(Guid redemptionId);

    /// <summary>
    /// Rejects a redemption (Admin only).
    /// </summary>
    /// <param name="redemptionId">Redemption ID</param>
    /// <param name="reason">Rejection reason</param>
    /// <returns>Result indicating success or error</returns>
    Task<Result> RejectRedemptionAsync(Guid redemptionId, string reason);

    /// <summary>
    /// Marks a redemption as delivered (Admin only).
    /// </summary>
    /// <param name="redemptionId">Redemption ID</param>
    /// <returns>Result indicating success or error</returns>
    Task<Result> MarkAsDeliveredAsync(Guid redemptionId);

    /// <summary>
    /// Cancels a redemption (owner or Admin only).
    /// </summary>
    /// <param name="redemptionId">Redemption ID</param>
    /// <returns>Result indicating success or error</returns>
    Task<Result> CancelRedemptionAsync(Guid redemptionId);
}
