using Microsoft.Extensions.Logging;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Services.Points
{
    /// <summary>
    /// Service implementation for points management operations.
    /// Handles all points manipulation with validation - Clean Architecture compliant.
    /// </summary>
    public class PointsManagementService : IPointsManagementService
    {
        private readonly IUserPointsAccountService _accountService;
        private readonly ILogger<PointsManagementService> _logger;

        public PointsManagementService(
            IUserPointsAccountService accountService,
            ILogger<PointsManagementService> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<PointsOperationResult> DeductPointsAsync(
            Guid userId, int points, string reason, Guid adminUserId)
        {
            try
            {
                // Validate first
                var validation = await ValidateDeductionAsync(userId, points);
                if (!validation.Success)
                {
                    return validation;
                }

                var account = await _accountService.GetAccountAsync(userId);
                
                // Perform deduction - the account handles the transaction internally
                account.DebitPoints(points, adminUserId);
                await _accountService.UpdateAccountAsync(account);

                _logger.LogInformation(
                    "Admin {AdminId} deducted {Points} points from user {UserId}. Reason: {Reason}",
                    adminUserId, points, userId, reason);

                return PointsOperationResult.Succeeded(account.CurrentBalance);
            }
            catch (KeyNotFoundException)
            {
                return PointsOperationResult.Failed($"User with ID {userId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting points from user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<PointsOperationResult> ValidateDeductionAsync(Guid userId, int points)
        {
            if (points <= 0)
            {
                return PointsOperationResult.Failed("Points must be greater than zero");
            }

            try
            {
                var account = await _accountService.GetAccountAsync(userId);
                
                if (!account.HasSufficientBalance(points))
                {
                    return PointsOperationResult.Failed(
                        $"Insufficient balance. Current balance: {account.CurrentBalance}, Required: {points}",
                        account.CurrentBalance);
                }

                return PointsOperationResult.Succeeded(account.CurrentBalance);
            }
            catch (KeyNotFoundException)
            {
                return PointsOperationResult.Failed($"Points account not found for user {userId}");
            }
        }
    }
}
