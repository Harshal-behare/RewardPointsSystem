using FluentValidation;
using RewardPointsSystem.Application.DTOs.Roles;

namespace RewardPointsSystem.Application.Validators.Roles
{
    /// <summary>
    /// Validator for assigning a role to a user
    /// </summary>
    public class AssignRoleDtoValidator : AbstractValidator<AssignRoleDto>
    {
        public AssignRoleDtoValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role ID is required");
        }
    }
}
