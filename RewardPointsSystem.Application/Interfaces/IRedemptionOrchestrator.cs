using System;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Operations;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IRedemptionOrchestrator
    /// Responsibility: Coordinate redemption flow only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IRedemptionOrchestrator
    {
        Task<RedemptionResult> ProcessRedemptionAsync(Guid userId, Guid productId);
        Task<Redemption> CreateRedemptionAsync(Guid userId, Guid productId, int quantity = 1);
        Task ApproveRedemptionAsync(Guid redemptionId, Guid approvedBy);
        Task RejectRedemptionAsync(Guid redemptionId, string reason);
        Task DeliverRedemptionAsync(Guid redemptionId, Guid processedBy, string? notes = null);
        Task MarkAsDeliveredAsync(Guid redemptionId, Guid processedBy);
        Task CancelRedemptionAsync(Guid redemptionId, string reason);
    }

    /// <summary>
    /// Result DTO for Redemption Processing
    /// </summary>
    public class RedemptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Redemption Redemption { get; set; }
        public UserPointsTransaction Transaction { get; set; }
    }
}