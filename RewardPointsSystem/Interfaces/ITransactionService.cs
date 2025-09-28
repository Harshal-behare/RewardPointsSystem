using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Interfaces
{
    public interface ITransactionService
    {
        Task RecordEarnedPointsAsync(Guid userId, int points, Guid eventId, string description);
        Task RecordRedeemedPointsAsync(Guid userId, int points, Guid redemptionId, string description);
        Task<IEnumerable<PointsTransaction>> GetUserTransactionsAsync(Guid userId);
        Task<IEnumerable<PointsTransaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to);
    }
}