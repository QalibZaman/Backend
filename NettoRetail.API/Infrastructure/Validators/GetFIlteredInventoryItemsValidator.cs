namespace NettoRetail.API.Infrastructure.Validators;

using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.InventoryItem;

public class GetFilteredInventoryItemsValidator : AbstractValidator<GetFilteredInventoryItemsModel>
{
    public GetFilteredInventoryItemsValidator()
    {
        RuleFor(x => x.ProductName)
            .MaximumLength(50).WithMessage("Product name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Product name must only contain letters, digits, '-' and '_'.");

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

        RuleForEach(x => x.SortingOptions).ChildRules(sorting =>
        {
            sorting.RuleFor(x => x.Field)
                .NotEmpty().WithMessage("Sorting field is required.");
        });
    }
}
