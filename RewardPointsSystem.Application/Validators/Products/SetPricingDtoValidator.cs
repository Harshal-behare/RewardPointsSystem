using FluentValidation;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Validators.Products
{
    /// <summary>
    /// Validator for setting product pricing
    /// </summary>
    public class SetPricingDtoValidator : AbstractValidator<SetPricingDto>
    {
        public SetPricingDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.PointsCost)
                .GreaterThan(0).WithMessage("Points cost must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Points cost cannot exceed 1,000,000");

            RuleFor(x => x.EffectiveDate)
                .NotEmpty().WithMessage("Effective date is required")
                .Must(BeValidDate).WithMessage("Effective date must be a valid date");
        }

        private bool BeValidDate(DateTime date)
        {
            return date != default && date >= DateTime.UtcNow.AddDays(-1);
        }
    }
}
