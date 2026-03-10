using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddInventoryItemValidator : AbstractValidator<AddInventoryItemModel>
{
    public AddInventoryItemValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage(Constants.Validation.Inventory.IsRequired)
            .NotNull().WithMessage(Constants.Validation.Inventory.IsRequired)
            .MinimumLength(Constants.Validation.Inventory.ProductNameMinLength).WithMessage(Constants.Validation.Inventory.NameMinLength)
            .MaximumLength(Constants.Validation.Inventory.ProductNameMaxLength).WithMessage(Constants.Validation.Inventory.NameMaxLength)
            .Matches(Constants.Validation.Inventory.ProductNameRegex).WithMessage(Constants.Validation.Inventory.ProductNameAllowedCharactersRegex);

        RuleFor(x => x.CategoryId).GreaterThan(Constants.Validation.Inventory.InvalidThreshold);

        RuleFor(x => x.BrandId).GreaterThan(Constants.Validation.Inventory.InvalidThreshold);

        RuleFor(x => x.ProductUpc)
            .NotEmpty().WithMessage(Constants.Validation.Inventory.UpcNotEmpty)
            .Matches(Constants.Validation.Inventory.UpcCodeRegex).WithMessage(Constants.Validation.Inventory.UpcMinLength);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(Constants.Validation.Inventory.InvalidThreshold).WithMessage(Constants.Validation.Inventory.PriceMinlength)
            .PrecisionScale(Constants.Validation.DatabaseConstants.FinancialPrecision, Constants.Validation.DatabaseConstants.FinancialScale, false).WithMessage(Constants.Validation.Inventory.InvalidPriceScaleMessage);

        RuleFor(x => x.Description)
            .MaximumLength(Constants.Validation.Inventory.DescriptionMaxLength).WithMessage(Constants.Validation.Inventory.DescriptionMaxLengthMessage)
            .Matches(Constants.Validation.Inventory.DescriptionRegex)
            .WithMessage(Constants.Validation.Inventory.DescriptionInvalidFormatMessage);

        RuleForEach(x => x.ProductDetails).SetValidator(new ProductDetailsModelValidator());
    }
}
