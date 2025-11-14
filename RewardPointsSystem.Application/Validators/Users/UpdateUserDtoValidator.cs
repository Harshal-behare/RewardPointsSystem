using FluentValidation;
using RewardPointsSystem.Application.DTOs;

namespace RewardPointsSystem.Application.Validators.Users
{
    /// <summary>
    /// Validator for updating user information
    /// </summary>
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            // All fields are optional for updates, but if provided, must be valid
            When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
            {
                RuleFor(x => x.FirstName)
                    .Length(1, 100).WithMessage("First name must be between 1 and 100 characters")
                    .Matches("^[a-zA-Z ]+$").WithMessage("First name can only contain letters and spaces");
            });

            When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
            {
                RuleFor(x => x.LastName)
                    .Length(1, 100).WithMessage("Last name must be between 1 and 100 characters")
                    .Matches("^[a-zA-Z ]+$").WithMessage("Last name can only contain letters and spaces");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
            {
                RuleFor(x => x.Email)
                    .EmailAddress().WithMessage("Invalid email format")
                    .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
            });
        }
    }
}
