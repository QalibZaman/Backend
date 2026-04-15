using FluentValidation;
using NettoRetail.Application.Models.Request.Variation;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateVariationsRequestValidator : AbstractValidator<CreateVariationsRequest>
{
    public CreateVariationsRequestValidator()
    {
        RuleFor(x => x.InventoryItemId)
            .NotEmpty().WithMessage("Parent inventory item id is required.");

        RuleFor(x => x.Variations)
            .NotNull().WithMessage("Variations are required.")
            .NotEmpty().WithMessage("At least one variation is required.");

        RuleForEach(x => x.Variations)
            .NotNull().WithMessage("Variation cannot be null.")
            .SetValidator(new VariationModelValidator());

        RuleFor(x => x.Variations)
            .Must(HaveUniqueVariationNames)
            .When(x => x.Variations is not null)
            .WithMessage("Variation names must be unique.");

        RuleFor(x => x.Variations)
            .Must(HaveUniqueNonEmptyUpcs)
            .When(x => x.Variations is not null)
            .WithMessage("Variation UPC values must be unique when provided.");
    }

    private static bool HaveUniqueVariationNames(ICollection<VariationModel> variations)
    {
        var names = variations
            .Where(v => v is not null)
            .Select(v => v.ProductName.Trim());

        return names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Count();
    }

    private static bool HaveUniqueNonEmptyUpcs(ICollection<VariationModel> variations)
    {
        var upcs = variations
            .Where(v => v is not null && !string.IsNullOrWhiteSpace(v.ProductUpc))
            .Select(v => v.ProductUpc.Trim());

        return upcs.Distinct(StringComparer.Ordinal).Count() == upcs.Count();
    }

    private sealed class VariationModelValidator : AbstractValidator<VariationModel>
    {
        public VariationModelValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Variation name is required.")
                .Length(2, 50).WithMessage("Variation name must be between 2 and 50 characters.");

            RuleFor(x => x.ProductUpc)
                .Matches("^\\d{12}$")
                .When(x => !string.IsNullOrWhiteSpace(x.ProductUpc))
                .WithMessage("Product UPC must be exactly 12 digits long when provided.");
        }
    }
}
