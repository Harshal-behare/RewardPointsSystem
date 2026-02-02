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

            // TotalPointsPool is auto-calculated from rank points, but validate if provided directly
            RuleFor(x => x.TotalPointsPool)
                .GreaterThan(0).WithMessage("Points pool must be greater than 0")
                .LessThanOrEqualTo(1000000).WithMessage("Points pool cannot exceed 1,000,000")
                .When(x => x.TotalPointsPool > 0);

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

            // Validate that if any rank points are provided, all three must be provided
            RuleFor(x => x)
                .Must(x => {
                    var hasFirst = x.FirstPlacePoints.HasValue && x.FirstPlacePoints > 0;
                    var hasSecond = x.SecondPlacePoints.HasValue && x.SecondPlacePoints > 0;
                    var hasThird = x.ThirdPlacePoints.HasValue && x.ThirdPlacePoints > 0;

                    // Either all are provided or none
                    if (hasFirst || hasSecond || hasThird)
                    {
                        return hasFirst && hasSecond && hasThird;
                    }
                    return true;
                })
                .WithMessage("If providing rank points, all three places (1st, 2nd, 3rd) must be specified");
        }
    }
}
