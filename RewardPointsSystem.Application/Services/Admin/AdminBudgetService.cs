using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RewardPointsSystem.Application.DTOs.Admin;
using RewardPointsSystem.Application.Interfaces;
using RewardPointsSystem.Domain.Entities.Core;

namespace RewardPointsSystem.Application.Services.Admin
{
    /// <summary>
    /// Service: AdminBudgetService
    /// Responsibility: Manage admin monthly points budget tracking
    /// </summary>
    public class AdminBudgetService : IAdminBudgetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminBudgetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<AdminBudgetResponseDto?> GetCurrentBudgetAsync(Guid adminUserId)
        {
            var currentMonthYear = AdminMonthlyBudget.GetCurrentMonthYear();
            var budget = await _unitOfWork.AdminMonthlyBudgets.SingleOrDefaultAsync(
                b => b.AdminUserId == adminUserId && b.MonthYear == currentMonthYear);

            if (budget == null)
                return null;

            return MapToResponseDto(budget);
        }

        public async Task<AdminBudgetResponseDto> SetBudgetAsync(Guid adminUserId, SetBudgetDto dto)
        {
            if (dto.BudgetLimit <= 0)
                throw new ArgumentException("Budget limit must be positive.", nameof(dto.BudgetLimit));

            if (dto.WarningThreshold < 0 || dto.WarningThreshold > 100)
                throw new ArgumentException("Warning threshold must be between 0 and 100.", nameof(dto.WarningThreshold));

            var currentMonthYear = AdminMonthlyBudget.GetCurrentMonthYear();
            var existingBudget = await _unitOfWork.AdminMonthlyBudgets.SingleOrDefaultAsync(
                b => b.AdminUserId == adminUserId && b.MonthYear == currentMonthYear);

            if (existingBudget != null)
            {
                // Update existing budget
                existingBudget.UpdateSettings(dto.BudgetLimit, dto.IsHardLimit, dto.WarningThreshold);
                await _unitOfWork.SaveChangesAsync();
                return MapToResponseDto(existingBudget);
            }

            // Create new budget
            var newBudget = AdminMonthlyBudget.Create(
                adminUserId,
                currentMonthYear,
                dto.BudgetLimit,
                dto.IsHardLimit,
                dto.WarningThreshold);

            await _unitOfWork.AdminMonthlyBudgets.AddAsync(newBudget);
            await _unitOfWork.SaveChangesAsync();

            return MapToResponseDto(newBudget);
        }

        public async Task<IEnumerable<BudgetHistoryItemDto>> GetBudgetHistoryAsync(Guid adminUserId, int months = 12)
        {
            var now = DateTime.UtcNow;
            var startMonthYear = (now.Year * 100 + now.Month) - months;

            var budgets = await _unitOfWork.AdminMonthlyBudgets.FindAsync(
                b => b.AdminUserId == adminUserId && b.MonthYear >= startMonthYear);

            return budgets
                .OrderByDescending(b => b.MonthYear)
                .Select(b => new BudgetHistoryItemDto
                {
                    MonthYear = b.MonthYear,
                    MonthYearDisplay = FormatMonthYear(b.MonthYear),
                    BudgetLimit = b.BudgetLimit,
                    PointsAwarded = b.PointsAwarded,
                    RemainingBudget = b.RemainingBudget,
                    UsagePercentage = Math.Round(b.UsagePercentage, 1),
                    WasOverBudget = b.IsOverBudget
                });
        }

        public async Task RecordPointsAwardedAsync(Guid adminUserId, int points)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be positive.", nameof(points));

            var budget = await GetOrCreateCurrentMonthBudgetAsync(adminUserId);
            if (budget == null)
                return; // No budget tracking for this admin

            budget.RecordPointsAwarded(points);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BudgetValidationResult> ValidatePointsAwardAsync(Guid adminUserId, int points)
        {
            var budget = await GetOrCreateCurrentMonthBudgetAsync(adminUserId);

            // No budget set - always allowed
            if (budget == null)
            {
                return new BudgetValidationResult
                {
                    IsAllowed = true,
                    IsWarning = false,
                    Message = null,
                    RemainingBudget = int.MaxValue,
                    PointsToAward = points,
                    PointsAfterAward = points
                };
            }

            var wouldExceed = budget.WouldExceedBudget(points);
            var newTotal = budget.PointsAwarded + points;
            var newPercentage = budget.BudgetLimit > 0 ? (double)newTotal / budget.BudgetLimit * 100 : 0;

            // Hard limit exceeded - blocked
            if (wouldExceed && budget.IsHardLimit)
            {
                return new BudgetValidationResult
                {
                    IsAllowed = false,
                    IsWarning = false,
                    Message = $"Cannot award {points} points. Would exceed hard budget limit of {budget.BudgetLimit}. Remaining: {budget.RemainingBudget}",
                    RemainingBudget = budget.RemainingBudget,
                    PointsToAward = points,
                    PointsAfterAward = newTotal
                };
            }

            // Soft limit exceeded - warning
            if (wouldExceed)
            {
                return new BudgetValidationResult
                {
                    IsAllowed = true,
                    IsWarning = true,
                    Message = $"Warning: Awarding {points} points will exceed your monthly budget of {budget.BudgetLimit}. New total: {newTotal}",
                    RemainingBudget = budget.RemainingBudget,
                    PointsToAward = points,
                    PointsAfterAward = newTotal
                };
            }

            // Warning zone (approaching threshold)
            if (newPercentage >= budget.WarningThreshold)
            {
                return new BudgetValidationResult
                {
                    IsAllowed = true,
                    IsWarning = true,
                    Message = $"Notice: After this award, you will have used {newPercentage:F1}% of your monthly budget ({newTotal}/{budget.BudgetLimit})",
                    RemainingBudget = budget.RemainingBudget - points,
                    PointsToAward = points,
                    PointsAfterAward = newTotal
                };
            }

            // Within budget
            return new BudgetValidationResult
            {
                IsAllowed = true,
                IsWarning = false,
                Message = null,
                RemainingBudget = budget.RemainingBudget - points,
                PointsToAward = points,
                PointsAfterAward = newTotal
            };
        }

        public async Task<AdminMonthlyBudget?> GetOrCreateCurrentMonthBudgetAsync(Guid adminUserId)
        {
            var currentMonthYear = AdminMonthlyBudget.GetCurrentMonthYear();
            return await _unitOfWork.AdminMonthlyBudgets.SingleOrDefaultAsync(
                b => b.AdminUserId == adminUserId && b.MonthYear == currentMonthYear);
        }

        private static AdminBudgetResponseDto MapToResponseDto(AdminMonthlyBudget budget)
        {
            return new AdminBudgetResponseDto
            {
                Id = budget.Id,
                MonthYear = budget.MonthYear,
                MonthYearDisplay = FormatMonthYear(budget.MonthYear),
                BudgetLimit = budget.BudgetLimit,
                PointsAwarded = budget.PointsAwarded,
                RemainingBudget = budget.RemainingBudget,
                UsagePercentage = Math.Round(budget.UsagePercentage, 1),
                IsHardLimit = budget.IsHardLimit,
                WarningThreshold = budget.WarningThreshold,
                IsOverBudget = budget.IsOverBudget,
                IsWarningZone = budget.IsWarningZone,
                CreatedAt = budget.CreatedAt,
                UpdatedAt = budget.UpdatedAt
            };
        }

        private static string FormatMonthYear(int monthYear)
        {
            var year = monthYear / 100;
            var month = monthYear % 100;
            var date = new DateTime(year, month, 1);
            return date.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
        }
    }
}
