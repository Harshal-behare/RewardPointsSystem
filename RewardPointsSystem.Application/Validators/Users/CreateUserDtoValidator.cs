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
        private readonly IUserService _userService;

        public CreateUserDtoValidator(IUserService userService)
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
