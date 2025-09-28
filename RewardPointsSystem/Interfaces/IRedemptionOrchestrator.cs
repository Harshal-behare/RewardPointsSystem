using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Operations;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Interfaces
{
    public interface IRedemptionOrchestrator
    {
        Task<RedemptionResult> ProcessRedemptionAsync(Guid userId, Guid productId, int quantity);
        Task<RedemptionResult> ValidateRedemptionAsync(Guid userId, Guid productId, int quantity);
        Task<IEnumerable<Redemption>> GetUserRedemptionsAsync(Guid userId);
        Task<RedemptionSummary> GetRedemptionSummaryAsync(DateTime from, DateTime to);
        Task<bool> CancelRedemptionAsync(Guid redemptionId, string reason);
    }

    public class RedemptionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Redemption Redemption { get; set; }
        public PointsTransaction Transaction { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class RedemptionSummary
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal TotalPointsSpent { get; set; }
        public decimal TotalValue { get; set; }
        public Dictionary<Guid, int> ProductRedemptionCounts { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}