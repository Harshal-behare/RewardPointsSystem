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
        Task ApproveRedemptionAsync(Guid redemptionId);
        Task DeliverRedemptionAsync(Guid redemptionId, string notes);
        Task CancelRedemptionAsync(Guid redemptionId);
    }

    /// <summary>
    /// Result DTO for Redemption Processing
    /// </summary>
    public class RedemptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Redemption Redemption { get; set; }
        public PointsTransaction Transaction { get; set; }
    }
}