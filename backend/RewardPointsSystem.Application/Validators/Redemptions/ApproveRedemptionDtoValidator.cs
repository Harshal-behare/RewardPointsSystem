using FluentValidation;
using RewardPointsSystem.Application.DTOs.Redemptions;

namespace RewardPointsSystem.Application.Validators.Redemptions
{
    /// <summary>
    /// Validator for approving a redemption
    /// </summary>
    public class ApproveRedemptionDtoValidator : AbstractValidator<ApproveRedemptionDto>
    {
        public ApproveRedemptionDtoValidator()
        {
            // Notes is optional - controller gets RedemptionId from route and ApprovedBy from JWT claims
            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
        }
    }
}
