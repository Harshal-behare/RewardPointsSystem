using System;
using System.ComponentModel.DataAnnotations;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;

namespace RewardPointsSystem.Domain.Entities.Operations
{
    public enum RedemptionStatus
    {
        Pending,
        Approved,
        Delivered,
        Cancelled
    }

    /// <summary>
    /// Represents a product redemption request by a user
    /// </summary>
    public class Redemption
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Points spent is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points spent must be a positive value")]
        public int PointsSpent { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        public RedemptionStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Guid? ProcessedBy { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [StringLength(1000, ErrorMessage = "Delivery notes cannot exceed 1000 characters")]
        public string? DeliveryNotes { get; set; }

        [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
        public string? RejectionReason { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
        public virtual User Approver { get; set; }
        public virtual User Processor { get; set; }

        public Redemption()
        {
            Id = Guid.NewGuid();
            Status = RedemptionStatus.Pending;
            RequestedAt = DateTime.UtcNow;
            Quantity = 1;
        }
    }
}
