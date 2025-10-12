using System;
using System.ComponentModel.DataAnnotations;

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

        public RedemptionStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        [StringLength(1000, ErrorMessage = "Delivery notes cannot exceed 1000 characters")]
        public string DeliveryNotes { get; set; }

        public Redemption()
        {
            Id = Guid.NewGuid();
            Status = RedemptionStatus.Pending;
            RequestedAt = DateTime.UtcNow;
        }
    }
}
