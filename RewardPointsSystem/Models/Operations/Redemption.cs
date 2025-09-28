using System;

namespace RewardPointsSystem.Models.Operations
{
    public enum RedemptionStatus
    {
        Pending,
        Approved,
        Delivered,
        Cancelled
    }

    public class Redemption
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int PointsSpent { get; set; }
        public RedemptionStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string DeliveryNotes { get; set; }

        public Redemption()
        {
            Id = Guid.NewGuid();
            Status = RedemptionStatus.Pending;
            RequestedAt = DateTime.UtcNow;
        }
    }
}