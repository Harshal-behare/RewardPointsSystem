using FluentValidation;
using RewardPointsSystem.Application.DTOs.Points;

namespace RewardPointsSystem.Application.Validators.Points
{
    /// <summary>
    /// Validator for deducting points from a user account
    /// </summary>
    public class DeductPointsDtoValidator : AbstractValidator<DeductPointsDto>
    {
        public DeductPointsDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Points)
                .GreaterThan(0).WithMessage("Points must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Points cannot exceed 100,000 per transaction");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");
        }
    }
}
