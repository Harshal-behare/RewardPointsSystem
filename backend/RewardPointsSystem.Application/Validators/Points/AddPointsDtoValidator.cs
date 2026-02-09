using FluentValidation;
using RewardPointsSystem.Application.DTOs.Points;

namespace RewardPointsSystem.Application.Validators.Points
{
    /// <summary>
    /// Validator for adding points to a user account
    /// </summary>
    public class AddPointsDtoValidator : AbstractValidator<AddPointsDto>
    {
        public AddPointsDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Points)
                .GreaterThan(0).WithMessage("Points must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Points cannot exceed 100,000 per transaction");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
