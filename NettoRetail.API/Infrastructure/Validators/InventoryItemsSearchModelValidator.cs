namespace NettoRetail.API.Infrastructure.Validators;

using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Utilities;

public class InventoryItemsSearchModelValidator : AbstractValidator<InventoryItemsSearchModel>
{
    public InventoryItemsSearchModelValidator()
    {
        RuleFor(x => x.UpcOrName)
            .NotEmpty().WithMessage("Please enter upc or name.")
            .Matches(@"^[a-zA-Z0-9\-_]{1,50}$")
            .WithMessage("Input must be either a valid UPC (up to 12 digits) or a valid product name (up to 50 characters, only letters, digits, '-' and '_').")
            .When(x => !string.IsNullOrWhiteSpace(x.UpcOrName));

        RuleFor(x => x.ProductName)
            .MinimumLength(2).WithMessage("Product name length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("Product name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_ ]*$").WithMessage("Product name must only contain letters, digits, spaces, '-' and '_'.").When(x => !string.IsNullOrWhiteSpace(x.ProductName));

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
            .GreaterThanOrEqualTo(x => x.MinAvailableAmount).WithMessage("Max available amount must be greater than or equal to min available amount.");

        RuleFor(x => x.MinReservedAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Min reserved amount must be 0 or greater.");

        RuleFor(x => x.MaxReservedAmount)
            .GreaterThanOrEqualTo(x => x.MinReservedAmount).WithMessage("Max reserved amount must be greater than or equal to min reserved amount.");

        RuleFor(x => x.MinOnHandAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Min on-hand amount must be 0 or greater.");

        RuleFor(x => x.MaxOnHandAmount)
            .GreaterThanOrEqualTo(x => x.MinOnHandAmount).WithMessage("Max on-hand amount must be greater than or equal to min on-hand amount.");

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Min price must be 0 or greater.")
            .PrecisionScale(18, 2, false).WithMessage("Min price must not have more than 2 decimal places.");

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice).WithMessage("Max price must be greater than or equal to min price.")
            .PrecisionScale(18, 2, false).WithMessage("Max price must not have more than 2 decimal places.");

        RuleFor(x => x.Status)
            .MustBeAValidInventoryItemStatus().WithMessage("Invalid status value provided.");

        RuleFor(x => x.BrandName)
            .MaximumLength(90).WithMessage("Brand name length must be less than 90 characters long.")
            .Matches("^[a-zA-Z0-9 _-]*$").WithMessage("Brand name must only contain letters, digits, spaces, '-' and '_'.");

        RuleForEach(x => x.SortingOptions).ChildRules(sorting =>
        {
            sorting.RuleFor(x => x.Field)
                .NotEmpty().WithMessage("Sorting field is required.");
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
