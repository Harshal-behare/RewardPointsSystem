using System;
using System.Threading.Tasks;
using RewardPointsSystem.Domain.Entities.Accounts;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IUserPointsAccountService
    /// Responsibility: Manage user point balances only
    /// Architecture Compliant - SRP
    /// </summary>
    public interface IUserPointsAccountService
    {
        Task<UserPointsAccount> CreateAccountAsync(Guid userId);
        Task<UserPointsAccount> GetAccountAsync(Guid userId);
        Task<int> GetBalanceAsync(Guid userId);
        Task AddUserPointsAsync(Guid userId, int userPoints);
        Task DeductUserPointsAsync(Guid userId, int userPoints);
        Task<bool> HasSufficientBalanceAsync(Guid userId, int requiredUserPoints);
        Task<IEnumerable<UserPointsAccount>> GetAllAccountsAsync();
        Task<IEnumerable<UserPointsAccount>> GetTopAccountsAsync(int count);
        Task UpdateAccountAsync(UserPointsAccount account);
    }
}