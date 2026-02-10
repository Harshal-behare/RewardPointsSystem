using FluentValidation;
using RewardPointsSystem.Application.DTOs.Auth;

namespace RewardPointsSystem.Application.Validators.Auth
{
    /// <summary>
    /// Validator for refresh token request
    /// </summary>
    public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required")
                .MinimumLength(20).WithMessage("Invalid refresh token format");
        }
    }
}
