namespace NettoRetail.API.Infrastructure.Validators;

using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Utilities;
using NettoRetail.Shared.Constants;

public class InventoryItemsSearchModelValidator : AbstractValidator<InventoryItemsSearchModel>
{
    public InventoryItemsSearchModelValidator()
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
            .WithMessage(ValidationErrorMessages.Inventory.ProductNameAllowedCharactersRegex)
            .When(x => !string.IsNullOrWhiteSpace(x.ProductName));

        RuleFor(x => x.ProductLocation)
            .NotEmpty().WithMessage("Product location is required.")
            .MinimumLength(2).WithMessage("Product location must be at least 2 characters long.")
            .MaximumLength(100).WithMessage("Product location must be at most 100 characters long.")
            .Matches(@"^(?! )[a-zA-Z0-9,\- ]*(?<! )$")
            .WithMessage("Product location may only contain letters, digits, spaces, hyphens (-), and commas (,), and cannot start or end with a space.")
            .When(x => !string.IsNullOrWhiteSpace(x.ProductLocation));

        RuleFor(x => x.CategoryId)
            .GreaterThanOrEqualTo(0).WithMessage("Category ID must be 0 or greater.");

        RuleFor(x => x.MinAvailableAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Min available amount must be 0 or greater.");

        RuleFor(x => x.MaxAvailableAmount)
            .GreaterThanOrEqualTo(x => x.MinAvailableAmount)
            .WithMessage("Max available amount must be greater than or equal to min available amount.");

        RuleFor(x => x.MinReservedAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Min reserved amount must be 0 or greater.");

        RuleFor(x => x.MaxReservedAmount)
            .GreaterThanOrEqualTo(x => x.MinReservedAmount)
            .WithMessage("Max reserved amount must be greater than or equal to min reserved amount.");

        RuleFor(x => x.MinOnHandAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Min on-hand amount must be 0 or greater.");

        RuleFor(x => x.MaxOnHandAmount)
            .GreaterThanOrEqualTo(x => x.MinOnHandAmount)
            .WithMessage("Max on-hand amount must be greater than or equal to min on-hand amount.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Min price must be 0 or greater.")
            .PrecisionScale(ValidationErrorMessages.Inventory.FinancialPrecision, ValidationErrorMessages.Inventory.FinancialScale, false)
            .WithMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice).WithMessage("Max price must be greater than or equal to min price.")
            .PrecisionScale(ValidationErrorMessages.Inventory.FinancialPrecision, ValidationErrorMessages.Inventory.FinancialScale, false)
            .WithMessage(ValidationErrorMessages.Inventory.InvalidPriceScaleMessage);

        RuleFor(x => x.Status)
            .MustBeAValidInventoryItemStatus()
            .WithMessage("Invalid status value provided.");

        RuleFor(x => x.BrandName)
            .MaximumLength(90).WithMessage("Brand name length must be less than 90 characters long.")
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Brand name must only contain letters, digits, '-' and '_'.");

        RuleForEach(x => x.SortingOptions).ChildRules(option =>
        {
            option.RuleFor(x => x.Field).NotEmpty().WithMessage("Sorting field is required.");
        });

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be greater than or equal to 1.");

        RuleFor(x => x)
            .Must(x => PaginationHelper.IsSkipWithinIntRange(x.PageNumber, x.PageSize))
            .WithMessage("Pagination parameters are too large.");
    }
}
