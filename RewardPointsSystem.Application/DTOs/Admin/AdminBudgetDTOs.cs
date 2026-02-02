namespace RewardPointsSystem.Application.DTOs.Admin
{
    /// <summary>
    /// Response DTO for admin budget status
    /// </summary>
    public class AdminBudgetResponseDto
    {
        public Guid Id { get; set; }
        public int MonthYear { get; set; }
        public string MonthYearDisplay { get; set; } = string.Empty;
        public int BudgetLimit { get; set; }
        public int PointsAwarded { get; set; }
        public int RemainingBudget { get; set; }
        public double UsagePercentage { get; set; }
        public bool IsHardLimit { get; set; }
        public int WarningThreshold { get; set; }
        public bool IsOverBudget { get; set; }
        public bool IsWarningZone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request DTO for setting/updating budget
    /// </summary>
    public class SetBudgetDto
    {
        public int BudgetLimit { get; set; }
        public bool IsHardLimit { get; set; } = false;
        public int WarningThreshold { get; set; } = 80;
    }

    /// <summary>
    /// Budget history item for monthly tracking
    /// </summary>
    public class BudgetHistoryItemDto
    {
        public int MonthYear { get; set; }
        public string MonthYearDisplay { get; set; } = string.Empty;
        public int BudgetLimit { get; set; }
        public int PointsAwarded { get; set; }
        public int RemainingBudget { get; set; }
        public double UsagePercentage { get; set; }
        public bool WasOverBudget { get; set; }
    }

    /// <summary>
    /// Budget validation result when awarding points
    /// </summary>
    public class BudgetValidationResult
    {
        public bool IsAllowed { get; set; }
        public bool IsWarning { get; set; }
        public string? Message { get; set; }
        public int RemainingBudget { get; set; }
        public int PointsToAward { get; set; }
        public int PointsAfterAward { get; set; }
    }
}
