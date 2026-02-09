using FluentValidation;
using RewardPointsSystem.Application.DTOs.Admin;

namespace RewardPointsSystem.Application.Validators.Admin
{
    /// <summary>
    /// Validator for setting admin monthly budget
    /// </summary>
    public class SetBudgetDtoValidator : AbstractValidator<SetBudgetDto>
    {
        public SetBudgetDtoValidator()
        {
            RuleFor(x => x.BudgetLimit)
                .GreaterThan(0).WithMessage("Budget limit must be greater than 0")
                .LessThanOrEqualTo(10000000).WithMessage("Budget limit cannot exceed 10,000,000 points");

            RuleFor(x => x.WarningThreshold)
                .InclusiveBetween(0, 100).WithMessage("Warning threshold must be between 0 and 100 percent");
        }
    }
}
