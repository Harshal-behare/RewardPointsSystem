using FluentValidation;
using RewardPointsSystem.Application.DTOs.Events;

namespace RewardPointsSystem.Application.Validators.Events
{
    /// <summary>
    /// Validator for registering a participant in an event
    /// </summary>
    public class RegisterParticipantDtoValidator : AbstractValidator<RegisterParticipantDto>
    {
        public RegisterParticipantDtoValidator()
        {
            RuleFor(x => x.EventId)
                .NotEmpty().WithMessage("Event ID is required");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }
}
