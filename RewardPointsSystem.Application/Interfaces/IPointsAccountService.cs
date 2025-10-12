using System;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IPointsAccountService
    /// Responsibility: Manage point balances only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IPointsAccountService
    {
        Task<PointsAccount> CreateAccountAsync(Guid userId);
        Task<PointsAccount> GetAccountAsync(Guid userId);
        Task<int> GetBalanceAsync(Guid userId);
        Task AddPointsAsync(Guid userId, int points);
        Task DeductPointsAsync(Guid userId, int points);
        Task<bool> HasSufficientBalanceAsync(Guid userId, int requiredPoints);
    }
}