using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Interfaces
{
    /// <summary>
    /// Interface: IAdminBudgetService
    /// Responsibility: Manage admin monthly points budget
    /// </summary>
    public interface IAdminBudgetService
    {
        /// <summary>
        /// Get current month's budget for an admin
        /// </summary>
        Task<AdminBudgetResponseDto?> GetCurrentBudgetAsync(Guid adminUserId);

        /// <summary>
        /// Set or update budget for current month
        /// </summary>
        Task<AdminBudgetResponseDto> SetBudgetAsync(Guid adminUserId, SetBudgetDto dto);

        /// <summary>
        /// Get budget history for last N months
        /// </summary>
        Task<IEnumerable<BudgetHistoryItemDto>> GetBudgetHistoryAsync(Guid adminUserId, int months = 12);

        /// <summary>
        /// Record points awarded (called when admin awards points)
        /// </summary>
        Task RecordPointsAwardedAsync(Guid adminUserId, int points);

        /// <summary>
        /// Validate if points can be awarded based on budget
        /// </summary>
        Task<BudgetValidationResult> ValidatePointsAwardAsync(Guid adminUserId, int points);

        /// <summary>
        /// Get or create budget for current month
        /// </summary>
        Task<AdminMonthlyBudget?> GetOrCreateCurrentMonthBudgetAsync(Guid adminUserId);
    }
}
