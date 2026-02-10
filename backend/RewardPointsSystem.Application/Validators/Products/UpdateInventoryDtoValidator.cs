using FluentValidation;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Validators.Products
{
    /// <summary>
    /// Validator for updating product inventory
    /// </summary>
    public class UpdateInventoryDtoValidator : AbstractValidator<UpdateInventoryDto>
    {
        public UpdateInventoryDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative")
                .LessThanOrEqualTo(100000).WithMessage("Quantity cannot exceed 100,000");

            RuleFor(x => x.ReorderLevel)
                .GreaterThanOrEqualTo(0).WithMessage("Reorder level cannot be negative")
                .LessThanOrEqualTo(10000).WithMessage("Reorder level cannot exceed 10,000");
        }
    }
}
