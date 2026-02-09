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

            // Allow any date for updates - the business logic in EventService handles state restrictions
            // Removing future date validation to allow updating past events

            When(x => x.TotalPointsPool.HasValue && x.TotalPointsPool > 0, () =>
            {
                RuleFor(x => x.TotalPointsPool)
                    .LessThanOrEqualTo(1000000).WithMessage("Points pool cannot exceed 1,000,000");
            });

            // Prize distribution validation: 1st > 2nd > 3rd
            RuleFor(x => x.FirstPlacePoints)
                .GreaterThan(0).WithMessage("First place points must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("First place points cannot exceed 1,000,000")
                .When(x => x.FirstPlacePoints.HasValue);

            RuleFor(x => x.SecondPlacePoints)
                .GreaterThan(0).WithMessage("Second place points must be greater than 0")
                .LessThan(x => x.FirstPlacePoints ?? int.MaxValue)
                    .WithMessage("Second place points must be less than first place points")
                .When(x => x.SecondPlacePoints.HasValue && x.FirstPlacePoints.HasValue);

            RuleFor(x => x.ThirdPlacePoints)
                .GreaterThan(0).WithMessage("Third place points must be greater than 0")
                .LessThan(x => x.SecondPlacePoints ?? int.MaxValue)
                    .WithMessage("Third place points must be less than second place points")
                .When(x => x.ThirdPlacePoints.HasValue && x.SecondPlacePoints.HasValue);
        }
    }
}
