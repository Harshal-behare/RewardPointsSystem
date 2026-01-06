using FluentValidation;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Validators.Events
{
    /// <summary>
    /// Validator for updating an event
    /// </summary>
    public class UpdateEventDtoValidator : AbstractValidator<UpdateEventDto>
    {
        public UpdateEventDtoValidator()
        {
            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .Length(3, 200).WithMessage("Event name must be between 3 and 200 characters");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");
            });

            When(x => x.EventDate != default, () =>
            {
                RuleFor(x => x.EventDate)
                    .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future");
            });

            When(x => x.TotalPointsPool > 0, () =>
            {
                RuleFor(x => x.TotalPointsPool)
                    .LessThanOrEqualTo(1000000).WithMessage("Points pool cannot exceed 1,000,000");
            });
        }
    }
}
