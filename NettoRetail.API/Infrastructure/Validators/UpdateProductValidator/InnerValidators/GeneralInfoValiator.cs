using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class GeneralInfoValidator : AbstractValidator<GeneralInfoModel>
{
    public GeneralInfoValidator()
    {
        RuleFor((model) => model)
            .Must(HasAnyPropToUpdate)
            .WithMessage(ValidationErrorMessages.ANY_PROP_UPDATE_ERROR_MESSAGE);

        When((model) => model.ProductName is not null, () =>
        {
            RuleFor((model) => model.ProductName)
                .MinimumLength(2)
                .WithMessage("Product name length must be at least 2 characters.")
                .MaximumLength(50)
                .WithMessage("Product name length must be at max 50 characters.")
                .Matches("^[a-zA-Z0-9-_]+$")
                .WithMessage("Allowed characters: letters (latin), digits and the following special characters: \"-\" and \"_\"");
        });

        When((model) => model.ProductUpc is not null, () =>
        {
            RuleFor((model) => model.ProductUpc)
                .Must(m => m!.Length == 12)
                .WithMessage("Product UPC must be 12 digits long.");
        });

        When((model) => model.Description is not null, () =>
        {
            RuleFor((model) => model.Description)
                .MaximumLength(500)
                .WithMessage("Description must be 500 characters at max.")
                .Must(d => !Regex.IsMatch(d, @"^\s+|\s+$"))
                .WithMessage("Description must not have leading or trailing space characters.")
                .Matches(@"^[A-Za-z0-9\s,.\-_'()]*$")
                .WithMessage("Description must only contain letters, digits, spaces, and common punctuation.");
        });
    }

    private static bool HasAnyPropToUpdate(GeneralInfoModel model)
    {
        return model.HasVariations is not null ||
            model.BrandId is not null ||
            model.ProductName is not null ||
            model.CategoryId is not null ||
            model.ProductUpc is not null ||
            model.Description is not null ||
            model.IsActive is not null;
    }
}