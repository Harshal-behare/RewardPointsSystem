using FluentValidation;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Validators.Products
{
    /// <summary>
    /// Validator for creating a new product
    /// </summary>
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

            RuleFor(x => x.PointsPrice)
                .GreaterThan(0).WithMessage("Points price must be greater than 0");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
                .Must(BeValidUrl).WithMessage("Image URL must be a valid URL")
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
