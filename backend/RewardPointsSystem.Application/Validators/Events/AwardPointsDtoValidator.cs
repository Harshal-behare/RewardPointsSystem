using FluentValidation;
using RewardPointsSystem.Application.DTOs.Events;

namespace RewardPointsSystem.Application.Validators.Events
{
    /// <summary>
    /// Validator for awarding points to event winners
    /// </summary>
    public class AwardPointsDtoValidator : AbstractValidator<AwardPointsDto>
    {
        public AwardPointsDtoValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty().WithMessage("Event ID is required");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Points)
                .GreaterThan(0).WithMessage("Points must be greater than 0")
                .LessThanOrEqualTo(100000).WithMessage("Points cannot exceed 100,000 per award");

            RuleFor(x => x.Position)
                .GreaterThan(0).WithMessage("Position must be at least 1")
                .LessThanOrEqualTo(100).WithMessage("Position cannot exceed 100");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
