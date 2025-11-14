using FluentValidation;

namespace RewardPointsSystem.Application.Validators.Common
{
    /// <summary>
    /// Validator for pagination parameters
    /// </summary>
    public class PaginationValidator : AbstractValidator<(int Page, int PageSize)>
    {
        public PaginationValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
        }
    }
}
