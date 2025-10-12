using System;
using System.Threading.Tasks;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Accounts;

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
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            var existingAccount = await _unitOfWork.PointsAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
            if (existingAccount != null)
                throw new InvalidOperationException($"Account already exists for user {userId}");

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
                throw new ArgumentException("Points must be greater than zero", nameof(points));

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
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            var account = await GetAccountAsync(userId);
            if (account == null)
                throw new InvalidOperationException($"Account not found for user {userId}");

            if (account.CurrentBalance < points)
                throw new InvalidOperationException($"Insufficient balance. Available: {account.CurrentBalance}, Required: {points}");

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