using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IUserPointsTransactionService
    /// Responsibility: Record user points transactions only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IUserPointsTransactionService
    {
        Task RecordEarnedUserPointsAsync(Guid userId, int userPoints, Guid eventId, string description);
        Task RecordRedeemedUserPointsAsync(Guid userId, int userPoints, Guid redemptionId, string description);
        Task<IEnumerable<UserPointsTransaction>> GetUserTransactionsAsync(Guid userId);
        Task<IEnumerable<UserPointsTransaction>> GetTransactionsByDateRangeAsync(DateTime from, DateTime to);
    }
}