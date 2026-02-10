using System;
using RewardPointsSystem.Domain.Entities.Core;
using RewardPointsSystem.Domain.Entities.Products;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Domain.Entities.Operations
{
    public enum RedemptionStatus
    {
        Pending,
        Approved,
        Delivered,
        Cancelled,
        Rejected
    }

    /// <summary>
    /// Represents a product redemption request with state machine logic
    /// </summary>
    public class Redemption
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public Guid ProductId { get; private set; }

        public int PointsSpent { get; private set; }

        public int Quantity { get; private set; }

        public RedemptionStatus Status { get; private set; }
        public DateTime RequestedAt { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public Guid? ApprovedBy { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public Guid? ProcessedBy { get; private set; }

        public string? RejectionReason { get; private set; }

       
        public virtual User? User { get; private set; }
        public virtual Product? Product { get; private set; }
        public virtual User? Approver { get; private set; }
        public virtual User? Processor { get; private set; }

        // EF Core requires a parameterless constructor
        private Redemption()
        {
        }

        private Redemption(
            Guid userId,
            Guid productId,
            int pointsSpent,
            int quantity)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            ProductId = productId;
            PointsSpent = ValidatePoints(pointsSpent);
            Quantity = ValidateQuantity(quantity);
            Status = RedemptionStatus.Pending;
            RequestedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new redemption request
        /// </summary>
        public static Redemption Create(
            Guid userId,
            Guid productId,
            int pointsSpent,
            int quantity)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (productId == Guid.Empty)
                throw new ArgumentException("Product ID cannot be empty.", nameof(productId));

            return new Redemption(userId, productId, pointsSpent, quantity);
        }

        /// <summary>
        /// Approves the redemption (Pending → Approved)
        /// </summary>
        public void Approve(Guid approvedBy)
        {
            if (Status != RedemptionStatus.Pending)
                throw new InvalidRedemptionStateException(Id, $"Cannot approve redemption with status {Status}.");

            if (approvedBy == Guid.Empty)
                throw new ArgumentException("Approver ID cannot be empty.", nameof(approvedBy));

            Status = RedemptionStatus.Approved;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
        }

        /// <summary>
        /// Marks the redemption as delivered (Approved → Delivered)
        /// </summary>
        public void Deliver(Guid deliveredBy)
        {
            if (Status != RedemptionStatus.Approved)
                throw new InvalidRedemptionStateException(Id, $"Cannot deliver redemption with status {Status}. Only approved redemptions can be delivered.");

            if (deliveredBy == Guid.Empty)
                throw new ArgumentException("Deliverer ID cannot be empty.", nameof(deliveredBy));

            Status = RedemptionStatus.Delivered;
            ProcessedAt = DateTime.UtcNow;
            ProcessedBy = deliveredBy;
        }

        /// <summary>
        /// Cancels the redemption (Pending/Approved → Cancelled) - Used for user cancellations
        /// </summary>
        public void Cancel(string reason)
        {
            if (Status == RedemptionStatus.Cancelled || Status == RedemptionStatus.Rejected)
                throw new InvalidRedemptionStateException(Id, "Redemption is already cancelled or rejected.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Cancellation reason is required.", nameof(reason));

            if (reason.Length > 500)
                throw new ArgumentException("Cancellation reason cannot exceed 500 characters.", nameof(reason));

            Status = RedemptionStatus.Cancelled;
            RejectionReason = reason.Trim();
        }

        /// <summary>
        /// Rejects the redemption (Pending → Rejected) - Used for admin rejections
        /// </summary>
        public void Reject(string reason)
        {
            if (Status != RedemptionStatus.Pending)
                throw new InvalidRedemptionStateException(Id, $"Only pending redemptions can be rejected. Current status: {Status}");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason is required.", nameof(reason));

            if (reason.Length > 500)
                throw new ArgumentException("Rejection reason cannot exceed 500 characters.", nameof(reason));

            Status = RedemptionStatus.Rejected;
            RejectionReason = reason.Trim();
        }

        /// <summary>
        /// Checks if redemption can be cancelled
        /// </summary>
        public bool CanBeCancelled()
        {
            return Status == RedemptionStatus.Pending || Status == RedemptionStatus.Approved;
        }

        /// <summary>
        /// Checks if redemption is in a final state
        /// </summary>
        public bool IsInFinalState()
        {
            return Status == RedemptionStatus.Delivered || Status == RedemptionStatus.Cancelled || Status == RedemptionStatus.Rejected;
        }

        /// <summary>
        /// Checks if redemption is pending approval
        /// </summary>
        public bool IsPendingApproval() => Status == RedemptionStatus.Pending;

        /// <summary>
        /// Gets total points for all items
        /// </summary>
        public int GetTotalPoints() => PointsSpent * Quantity;

        private static int ValidatePoints(int points)
        {
            if (points < 1)
                throw new ArgumentException("Points spent must be a positive value.", nameof(points));

            return points;
        }

        private static int ValidateQuantity(int quantity)
        {
            if (quantity < 1)
                throw new ArgumentException("Quantity must be at least 1.", nameof(quantity));

            return quantity;
        }
    }
}
