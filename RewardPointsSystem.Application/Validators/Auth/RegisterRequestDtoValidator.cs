using FluentValidation;
using RewardPointsSystem.Application.DTOs.Auth;
using RewardPointsSystem.Application.Interfaces;

namespace RewardPointsSystem.Application.Validators.Auth
{
    /// <summary>
    /// Validator for user registration
    /// </summary>
    public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        private readonly IUserService _userService;

        public RegisterRequestDtoValidator(IUserService userService)
        {
            _userService = userService;

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .Length(1, 100).WithMessage("First name must be between 1 and 100 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("First name can only contain letters and spaces");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .Length(1, 100).WithMessage("Last name must be between 1 and 100 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Last name can only contain letters and spaces");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters")
                .MustAsync(BeUniqueEmail).WithMessage("Email already exists");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
                .Matches(@"[\W_]").WithMessage("Password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password confirmation is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");
        }

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _userService.GetUserByEmailAsync(email);
                return existingUser == null;
            }
            catch
            {
                return true; // If service fails, allow validation to pass
            }
        }
    }
}
