using System;
using System.Threading.Tasks;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Interfaces
{
    /// <summary>
    /// Interface: IRewardAccountService
    /// Responsibility: Manage point balances only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IRewardAccountService
    {
        Task<RewardAccount> CreateAccountAsync(Guid userId);
        Task<RewardAccount> GetAccountAsync(Guid userId);
        Task<int> GetBalanceAsync(Guid userId);
        Task AddPointsAsync(Guid userId, int points);
        Task DeductPointsAsync(Guid userId, int points);
        Task<bool> HasSufficientBalanceAsync(Guid userId, int requiredPoints);
    }
}