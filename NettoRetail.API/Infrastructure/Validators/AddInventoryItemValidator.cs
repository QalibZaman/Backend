using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddInventoryItemValidator : AbstractValidator<AddInventoryItemModel>
{
    public AddInventoryItemValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage(ValidationErrorMessages.Inventory.IsRequired)
            .NotNull().WithMessage(ValidationErrorMessages.Inventory.IsRequired)
            .MinimumLength(ValidationErrorMessages.Inventory.ProductNameMinLength).WithMessage(ValidationErrorMessages.Inventory.NameMinLength)
            .MaximumLength(ValidationErrorMessages.Inventory.ProductNameMaxLength).WithMessage(ValidationErrorMessages.Inventory.NameMaxLength)
            .Matches(ValidationErrorMessages.Inventory.ProductNameRegex).WithMessage(ValidationErrorMessages.Inventory.ProductNameAllowedCharactersRegex);
        RuleFor(x => x.CategoryId).GreaterThan(ValidationErrorMessages.Inventory.InvalidThreshold);

        RuleFor(x => x.BrandId).GreaterThan(ValidationErrorMessages.Inventory.InvalidThreshold);

        RuleFor(x => x.ProductUpc)
            .NotEmpty().WithMessage(ValidationErrorMessages.Inventory.UpcNotEmptyErrorMessage)
            .Matches(ValidationErrorMessages.Inventory.UpcCodeRegex).WithMessage(ValidationErrorMessages.Inventory.UpcMinLengthErrorMessage);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(ValidationErrorMessages.Inventory.InvalidThreshold).WithMessage(ValidationErrorMessages.Inventory.PriceMinlength)
            .PrecisionScale(ValidationErrorMessages.Inventory.FinancialPrecision, ValidationErrorMessages.Inventory.FinancialScale, false).WithMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);

        RuleFor(x => x.Description)
            .MaximumLength(ValidationErrorMessages.Inventory.DescriptionMaxLength).WithMessage(ValidationErrorMessages.Inventory.DescriptionMaxLengthMessage)
            .Matches(ValidationErrorMessages.Inventory.DescriptionRegex)
            .WithMessage(ValidationErrorMessages.Inventory.DescriptionInvalidFormatMessage);

        RuleForEach(x => x.ProductDetails).SetValidator(new ProductDetailsModelValidator());
    }
}
