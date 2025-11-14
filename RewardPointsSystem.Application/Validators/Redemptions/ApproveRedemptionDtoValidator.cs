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
            RuleFor(x => x.RedemptionId)
                .NotEmpty().WithMessage("Redemption ID is required");

            RuleFor(x => x.ApprovedBy)
                .NotEmpty().WithMessage("Approver ID is required");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
        }
    }
}
