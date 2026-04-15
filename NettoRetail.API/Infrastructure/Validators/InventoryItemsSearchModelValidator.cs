namespace NettoRetail.API.Infrastructure.Validators;

using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Utilities;
using NettoRetail.Shared.Constants;

public class InventoryItemsSearchModelValidator : AbstractValidator<InventoryItemsSearchModel>
{
    private readonly IValidator<SortingOption> _validator;
    public InventoryItemsSearchModelValidator(IValidator<SortingOption> validator)
    {
        _validator = validator;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Upc)
            .NotEmpty().WithMessage(ValidationErrorMessages.Inventory.UpcNotEmptyErrorMessage)
            .Matches(ValidationErrorMessages.Inventory.UpcCodeRegex)
            .WithMessage(ValidationErrorMessages.Inventory.UpcMinLengthErrorMessage)
            .When(x => !string.IsNullOrWhiteSpace(x.Upc));

        RuleFor(x => x.ProductName)
            .MinimumLength(ValidationErrorMessages.Inventory.ProductNameMinLength)
            .WithMessage(ValidationErrorMessages.Inventory.NameMinLength)
            .MaximumLength(ValidationErrorMessages.Inventory.ProductNameMaxLength)
            .WithMessage(ValidationErrorMessages.Inventory.NameMaxLength)
            .Matches(ValidationErrorMessages.Inventory.ProductNameRegex)
            .WithMessage(ValidationErrorMessages.Inventory.ProductNameAllowedCharactersRegex).When(x => !string.IsNullOrWhiteSpace(x.ProductName));

        RuleFor(x => x.ProductLocation)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches(ValidationErrorMessages.Inventory.ProductLocationRegex)
            .WithMessage(ValidationErrorMessages.Inventory.ProductLocationInvalidFormatMessage)
            .When(x => !string.IsNullOrWhiteSpace(x.ProductLocation));

        RuleFor(x => x.CategoryId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MinAvailableAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MaxAvailableAmount)
            .GreaterThanOrEqualTo(x => x.MinAvailableAmount)
            .WithMessage(ValidationErrorMessages.Inventory.MaxAvailableAmountMinLengthMessage);

        RuleFor(x => x.MinReservedAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MaxReservedAmount)
            .GreaterThanOrEqualTo(x => x.MinReservedAmount)
            .WithMessage(ValidationErrorMessages.Inventory.MaxReservedAmountMinLengthMessage);

        RuleFor(x => x.MinOnHandAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MaxOnHandAmount)
            .GreaterThanOrEqualTo(x => x.MinOnHandAmount)
            .WithMessage(ValidationErrorMessages.Inventory.MaxOnHandAmountMinLengthMessage);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .PrecisionScale(ValidationErrorMessages.Inventory.FinancialPrecision, ValidationErrorMessages.Inventory.FinancialScale, false)
            .WithMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .WithMessage(ValidationErrorMessages.Inventory.MaxPriceMaxLengthErrorMessage)
            .PrecisionScale(ValidationErrorMessages.Inventory.FinancialPrecision, ValidationErrorMessages.Inventory.FinancialScale, false)
            .WithMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);

        RuleFor(x => x.Status)
            .MustBeAValidInventoryItemStatus()
            .WithMessage(ValidationErrorMessages.Inventory.InvalidStatusErrorMessage);

        RuleFor(x => x.BrandName)
            .MaximumLength(90)
            .Matches("^[a-zA-Z0-9-_]*$")
            .WithMessage(ValidationErrorMessages.Brand.BrandNameAllowedCharactersRegex);

        RuleForEach(x => x.SortingOptions)
            .SetValidator(_validator);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x)
            .Must(x => PaginationHelper.IsSkipWithinIntRange(x.PageNumber, x.PageSize))
            .WithMessage(ValidationErrorMessages.Pagination.PaginationTooLargeErrorMessage);
    }
}
