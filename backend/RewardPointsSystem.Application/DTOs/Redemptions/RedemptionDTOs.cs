using System;

namespace RewardPointsSystem.Application.DTOs.Redemptions
{
    /// <summary>
    /// DTO for creating a redemption.
    /// UserId is not included - it's derived from JWT/ICurrentUserContext in the Application layer.
    /// </summary>
    public class CreateRedemptionDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Basic redemption response DTO
    /// </summary>
    public class RedemptionResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int PointsSpent { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string RejectionReason { get; set; }  // Added for rejected/cancelled redemptions
    }

    /// <summary>
    /// Detailed redemption response
    /// </summary>
    public class RedemptionDetailsDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCategory { get; set; }
        public int PointsSpent { get; set; }
        public string Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// DTO for approving a redemption
    /// </summary>
    public class ApproveRedemptionDto
    {
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for cancelling a redemption
    /// </summary>
    public class CancelRedemptionDto
    {
        public string CancellationReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for rejecting a redemption
    /// </summary>
    public class RejectRedemptionDto
    {
        public string RejectionReason { get; set; }
    }

    /// <summary>
    /// DTO for returning count values
    /// </summary>
    public class PendingCountDto
    {
        public int Count { get; set; }
    }
}
