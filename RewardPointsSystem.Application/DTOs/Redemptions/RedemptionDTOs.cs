using System;

namespace RewardPointsSystem.Application.DTOs.Redemptions
{
    /// <summary>
    /// DTO for creating a redemption
    /// </summary>
    public class CreateRedemptionDto
    {
        public Guid UserId { get; set; }
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
        public DateTime? DeliveredAt { get; set; }
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
        public DateTime? DeliveredAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string DeliveryNotes { get; set; }
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
    /// DTO for delivering a redemption
    /// </summary>
    public class DeliverRedemptionDto
    {
        public string? DeliveryNotes { get; set; }
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
}
