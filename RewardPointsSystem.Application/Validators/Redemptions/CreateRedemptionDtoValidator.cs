using FluentValidation;
using RewardPointsSystem.Application.DTOs.Redemptions;

namespace RewardPointsSystem.Application.Validators.Redemptions
{
    /// <summary>
    /// Validator for creating a redemption request
    /// </summary>
    public class CreateRedemptionDtoValidator : AbstractValidator<CreateRedemptionDto>
    {
        public CreateRedemptionDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1")
                .LessThanOrEqualTo(10).WithMessage("Quantity cannot exceed 10 items per redemption");
        }
    }
}
