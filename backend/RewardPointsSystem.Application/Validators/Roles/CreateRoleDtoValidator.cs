using FluentValidation;
using RewardPointsSystem.Application.DTOs.Roles;

namespace RewardPointsSystem.Application.Validators.Roles
{
    /// <summary>
    /// Validator for creating a new role
    /// </summary>
    public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Role name is required")
                .Length(2, 50).WithMessage("Role name must be between 2 and 50 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Role name can only contain letters and spaces");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        }
    }
}
