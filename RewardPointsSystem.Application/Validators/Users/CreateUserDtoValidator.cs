using FluentValidation;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Validators.Users
{
    /// <summary>
    /// Validator for creating a new user
    /// </summary>
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 20).WithMessage("First name must be between 1 and 20 characters")
                .Matches("^[a-zA-Z][a-zA-Z ]*$").WithMessage("First name can only contain letters and spaces, and cannot start with a space");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 20).WithMessage("Last name must be between 1 and 20 characters")
                .Matches("^[a-zA-Z][a-zA-Z ]*$").WithMessage("Last name can only contain letters and spaces, and cannot start with a space");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .Must(email => email != null && email.ToLower().EndsWith("@agdata.com"))
                .WithMessage("Email must be an @agdata.com address");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
            // Note: Email uniqueness check is handled in the controller/service layer
        }
    }
}
