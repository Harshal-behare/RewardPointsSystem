using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Application.Services.Accounts
{
    public class UserPointsAccountService : IUserPointsAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserPointsAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<UserPointsAccount> CreateAccountAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);
            
            if (!user.IsActive)
                throw new InactiveUserException(userId);

            var existingAccount = await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
            if (existingAccount != null)
                throw new InvalidUserPointsOperationException($"Account already exists for user {userId}");

            var account = UserPointsAccount.CreateForUser(userId);

            await _unitOfWork.UserPointsAccounts.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return account;
        }

        public async Task<UserPointsAccount> GetAccountAsync(Guid userId)
        {
            return await _unitOfWork.UserPointsAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
        }

        public async Task<int> GetBalanceAsync(Guid userId)
        {
            var account = await GetAccountAsync(userId);
            return account?.CurrentBalance ?? 0;
        }

        public async Task AddUserPointsAsync(Guid userId, int userPoints)
        {
            if (userPoints <= 0)
                throw new InvalidUserPointsOperationException("User points must be greater than zero");

            var account = await GetAccountAsync(userId);
            if (account == null)
                account = await CreateAccountAsync(userId);

            account.CreditPoints(userPoints, userId);

            await _unitOfWork.UserPointsAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeductUserPointsAsync(Guid userId, int userPoints)
        {
            if (userPoints <= 0)
                throw new InvalidUserPointsOperationException("User points must be greater than zero");

            var account = await GetAccountAsync(userId);
            if (account == null)
                throw new UserPointsAccountNotFoundException(userId);

            account.DebitPoints(userPoints, userId);

            await _unitOfWork.UserPointsAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> HasSufficientBalanceAsync(Guid userId, int requiredUserPoints)
        {
            if (requiredUserPoints <= 0)
                return true;

            var balance = await GetBalanceAsync(userId);
            return balance >= requiredUserPoints;
        }

        public async Task<IEnumerable<UserPointsAccount>> GetAllAccountsAsync()
        {
            return await _unitOfWork.UserPointsAccounts.GetAllAsync();
        }

        public async Task<IEnumerable<UserPointsAccount>> GetTopAccountsAsync(int count)
        {
            var accounts = await _unitOfWork.UserPointsAccounts.GetAllAsync();
            return accounts.OrderByDescending(a => a.CurrentBalance).Take(count);
        }

        public async Task UpdateAccountAsync(UserPointsAccount account)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            await _unitOfWork.UserPointsAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
