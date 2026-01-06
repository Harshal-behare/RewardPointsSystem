using FluentValidation;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Validators.Events
{
    /// <summary>
    /// Validator for creating a new event
    /// </summary>
    public class CreateEventDtoValidator : AbstractValidator<CreateEventDto>
    {
        public CreateEventDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event name is required")
                .Length(3, 200).WithMessage("Event name must be between 3 and 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.EventDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Event date must be in the future");

            RuleFor(x => x.TotalPointsPool)
                .GreaterThan(0).WithMessage("Points pool must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Points pool cannot exceed 1,000,000");
        }
    }
}
