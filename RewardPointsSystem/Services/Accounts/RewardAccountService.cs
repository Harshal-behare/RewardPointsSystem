using System;
using System.Threading.Tasks;
using RewardPointsSystem.Interfaces;
using RewardPointsSystem.Models.Accounts;

namespace RewardPointsSystem.Services.Accounts
{
    public class RewardAccountService : IRewardAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RewardAccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<RewardAccount> CreateAccountAsync(Guid userId)
        {
            // Validate user exists and is active
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException($"User with ID {userId} not found or inactive");

            // Check if account already exists
            var existingAccount = await _unitOfWork.RewardAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
            if (existingAccount != null)
                throw new InvalidOperationException($"Reward account already exists for user {userId}");

            var account = new RewardAccount
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CurrentBalance = 0,
                TotalPointsEarned = 0,
                TotalPointsRedeemed = 0,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RewardAccounts.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return account;
        }

        public async Task<RewardAccount> GetAccountAsync(Guid userId)
        {
            var account = await _unitOfWork.RewardAccounts.SingleOrDefaultAsync(ra => ra.UserId == userId);
            
            // Auto-create account if it doesn't exist for active user
            if (account == null)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null && user.IsActive)
                {
                    account = await CreateAccountAsync(userId);
                }
            }

            return account;
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var account = await GetAccountAsync(userId);
            return account?.CurrentBalance ?? 0;
        }

        public async Task<RewardAccount> AddPointsAsync(Guid userId, decimal points)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            var account = await GetAccountAsync(userId);
            if (account == null)
                throw new InvalidOperationException($"No reward account found for user {userId}");

            account.CurrentBalance += points;
            account.TotalPointsEarned += points;
            account.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RewardAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return account;
        }

        public async Task<RewardAccount> DeductPointsAsync(Guid userId, decimal points)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be greater than zero", nameof(points));

            var account = await GetAccountAsync(userId);
            if (account == null)
                throw new InvalidOperationException($"No reward account found for user {userId}");

            if (account.CurrentBalance < points)
                throw new InvalidOperationException($"Insufficient balance. Available: {account.CurrentBalance}, Required: {points}");

            account.CurrentBalance -= points;
            account.TotalPointsRedeemed += points;
            account.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RewardAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return account;
        }

        public async Task<bool> HasSufficientBalanceAsync(Guid userId, decimal requiredPoints)
        {
            if (requiredPoints <= 0)
                return true;

            var balance = await GetBalanceAsync(userId);
            return balance >= requiredPoints;
        }

        public async Task<RewardAccount> UpdateAccountSummaryAsync(Guid userId)
        {
            var account = await GetAccountAsync(userId);
            if (account == null)
                throw new InvalidOperationException($"No reward account found for user {userId}");

            // Recalculate totals from transactions
            var transactions = await _unitOfWork.PointsTransactions.FindAsync(t => t.UserId == userId);
            
            decimal totalEarned = 0;
            decimal totalRedeemed = 0;
            decimal currentBalance = 0;

            foreach (var transaction in transactions)
            {
                if (transaction.Points > 0)
                {
                    totalEarned += transaction.Points;
                    currentBalance += transaction.Points;
                }
                else
                {
                    totalRedeemed += Math.Abs(transaction.Points);
                    currentBalance += transaction.Points; // transaction.Points is negative for redemptions
                }
            }

            account.CurrentBalance = currentBalance;
            account.TotalPointsEarned = totalEarned;
            account.TotalPointsRedeemed = totalRedeemed;
            account.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RewardAccounts.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return account;
        }

        public async Task<bool> ValidateAccountIntegrityAsync(Guid userId)
        {
            var account = await GetAccountAsync(userId);
            if (account == null)
                return false;

            // Validate that account balance matches transaction history
            var transactions = await _unitOfWork.PointsTransactions.FindAsync(t => t.UserId == userId);
            
            decimal calculatedBalance = 0;
            decimal calculatedEarned = 0;
            decimal calculatedRedeemed = 0;

            foreach (var transaction in transactions)
            {
                calculatedBalance += transaction.Points;
                
                if (transaction.Points > 0)
                    calculatedEarned += transaction.Points;
                else
                    calculatedRedeemed += Math.Abs(transaction.Points);
            }

            return account.CurrentBalance == calculatedBalance &&
                   account.TotalPointsEarned == calculatedEarned &&
                   account.TotalPointsRedeemed == calculatedRedeemed;
        }
    }
}