using System;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;
using RewardPointsSystem.Domain.Exceptions;

namespace RewardPointsSystem.Application.Services.Accounts
{
    public class PointsAccountService : IPointsAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PointsAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<PointsAccount> CreateAccountAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new UserNotFoundException(userId);
            
            if (!user.IsActive)
                throw new InactiveUserException(userId);

            var existingAccount = await _unitOfWork.PointsAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
            if (existingAccount != null)
                throw new InvalidPointsOperationException($"Account already exists for user {userId}");

            var account = new PointsAccount
            {
                UserId = userId,
                CurrentBalance = 0,
                TotalEarned = 0,
                TotalRedeemed = 0,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PointsAccounts.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return account;
        }

        public async Task<PointsAccount> GetAccountAsync(Guid userId)
        {
            return await _unitOfWork.PointsAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
        }

        public async Task<int> GetBalanceAsync(Guid userId)
        {
            var account = await GetAccountAsync(userId);
            return account?.CurrentBalance ?? 0;
        }

        public async Task AddPointsAsync(Guid userId, int points)
        {
            if (points <= 0)
                throw new InvalidPointsOperationException("Points must be greater than zero");

            var account = await GetAccountAsync(userId);
            if (account == null)
                account = await CreateAccountAsync(userId);

            account.CurrentBalance += points;
            account.TotalEarned += points;
            account.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PointsAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeductPointsAsync(Guid userId, int points)
        {
            if (points <= 0)
                throw new InvalidPointsOperationException("Points must be greater than zero");

            var account = await GetAccountAsync(userId);
            if (account == null)
                throw new PointsAccountNotFoundException(userId);

            if (account.CurrentBalance < points)
                throw new InsufficientPointsBalanceException(userId, points, account.CurrentBalance);

            account.CurrentBalance -= points;
            account.TotalRedeemed += points;
            account.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.PointsAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> HasSufficientBalanceAsync(Guid userId, int requiredPoints)
        {
            if (requiredPoints <= 0)
                return true;

            var balance = await GetBalanceAsync(userId);
            return balance >= requiredPoints;
        }
    }
}