using System;

namespace RewardPointsSystem.Domain.Entities.Core
{
    /// <summary>
    /// Tracks admin's monthly points budget for awarding
    /// </summary>
    public class AdminMonthlyBudget
    {
        public Guid Id { get; private set; }
        public Guid AdminUserId { get; private set; }
        public int MonthYear { get; private set; }  // YYYYMM format (e.g., 202602)
        public int BudgetLimit { get; private set; }
        public int PointsAwarded { get; private set; }
        public bool IsHardLimit { get; private set; }
        public int WarningThreshold { get; private set; }  // Percentage (default 80)
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // Navigation
        public virtual User AdminUser { get; private set; }

        // Computed properties
        public int RemainingBudget => Math.Max(0, BudgetLimit - PointsAwarded);
        public double UsagePercentage => BudgetLimit > 0 ? (double)PointsAwarded / BudgetLimit * 100 : 0;
        public bool IsOverBudget => PointsAwarded > BudgetLimit;
        public bool IsWarningZone => UsagePercentage >= WarningThreshold && !IsOverBudget;

        private AdminMonthlyBudget()
        {
            AdminUser = null!;
        }

        private AdminMonthlyBudget(Guid adminUserId, int monthYear, int budgetLimit, bool isHardLimit, int warningThreshold) : this()
        {
            Id = Guid.NewGuid();
            AdminUserId = adminUserId;
            MonthYear = monthYear;
            BudgetLimit = budgetLimit;
            PointsAwarded = 0;
            IsHardLimit = isHardLimit;
            WarningThreshold = warningThreshold;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Factory method to create a new admin monthly budget
        /// </summary>
        public static AdminMonthlyBudget Create(Guid adminUserId, int monthYear, int budgetLimit, bool isHardLimit = false, int warningThreshold = 80)
        {
            if (budgetLimit <= 0)
                throw new ArgumentException("Budget limit must be positive.", nameof(budgetLimit));

            if (warningThreshold < 0 || warningThreshold > 100)
                throw new ArgumentException("Warning threshold must be between 0 and 100.", nameof(warningThreshold));

            return new AdminMonthlyBudget(adminUserId, monthYear, budgetLimit, isHardLimit, warningThreshold);
        }

        /// <summary>
        /// Gets the current month-year in YYYYMM format
        /// </summary>
        public static int GetCurrentMonthYear()
        {
            var now = DateTime.UtcNow;
            return now.Year * 100 + now.Month;
        }

        /// <summary>
        /// Update budget settings
        /// </summary>
        public void UpdateSettings(int budgetLimit, bool isHardLimit, int warningThreshold)
        {
            if (budgetLimit <= 0)
                throw new ArgumentException("Budget limit must be positive.", nameof(budgetLimit));

            if (warningThreshold < 0 || warningThreshold > 100)
                throw new ArgumentException("Warning threshold must be between 0 and 100.", nameof(warningThreshold));

            BudgetLimit = budgetLimit;
            IsHardLimit = isHardLimit;
            WarningThreshold = warningThreshold;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Record points awarded
        /// </summary>
        public void RecordPointsAwarded(int points)
        {
            if (points <= 0)
                throw new ArgumentException("Points must be positive.", nameof(points));

            if (IsHardLimit && PointsAwarded + points > BudgetLimit)
                throw new InvalidOperationException($"Cannot award {points} points. Would exceed hard budget limit. Remaining: {RemainingBudget}");

            PointsAwarded += points;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if awarding points would exceed budget (for soft limit warning)
        /// </summary>
        public bool WouldExceedBudget(int points)
        {
            return PointsAwarded + points > BudgetLimit;
        }
    }
}
