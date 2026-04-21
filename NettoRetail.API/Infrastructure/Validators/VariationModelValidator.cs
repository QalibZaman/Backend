using FluentValidation;
using NettoRetail.Application.Models.Request.Variation;

namespace NettoRetail.API.Infrastructure.Validators;

public class VariationModelValidator : AbstractValidator<VariationModel>
{
    public VariationModelValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .Matches(@"^[A-Za-z0-9_-]{2,50}$").WithMessage("Product name must be between 2 and 50 characters and contain only letters, digits, '-' or '_'.");

        RuleFor(x => x.ProductUpc)
            .NotEmpty().WithMessage("Product UPC is required.")
            .Matches(@"^\d{12}$").WithMessage("Product UPC must be exactly 12 digits long.");
    }
}
