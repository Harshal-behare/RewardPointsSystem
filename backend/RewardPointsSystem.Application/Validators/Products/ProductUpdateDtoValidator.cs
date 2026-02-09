using FluentValidation;
using RewardPointsSystem.Application.DTOs;
using RewardPointsSystem.Application.DTOs.Products;

namespace RewardPointsSystem.Application.Validators.Products
{
    /// <summary>
    /// Validator for updating a product
    /// </summary>
    public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateDtoValidator()
        {
            When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .Length(2, 200).WithMessage("Product name must be between 2 and 200 characters");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
            });

            When(x => !string.IsNullOrWhiteSpace(x.ImageUrl), () =>
            {
                RuleFor(x => x.ImageUrl)
                    .MaximumLength(500).WithMessage("Image URL cannot exceed 500 characters")
                    .Must(BeValidUrl).WithMessage("Image URL must be a valid URL");
            });
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
