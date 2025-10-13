using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Shop;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetFilteredCatalogValidator : AbstractValidator<GetFilteredCatalogModel>
{
    public GetFilteredCatalogValidator()
    {
        RuleForEach(x => x.BrandsNames).ChildRules(x =>
        {
            x.RuleFor(v => v)
                .MaximumLength(50).WithMessage("Brand name length must be less than 50 characters long.")
                .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Brand name must only contain letters, digits, '-' and '_'.");
        });

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Min price must be 0 or greater.")
            .PrecisionScale(18, 2, false).WithMessage("Min price must not have more than 2 decimal places.");

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0).WithMessage("Max price must be greater than 0.")
            .GreaterThanOrEqualTo(x => x.MinPrice).When(x => x.MinPrice.HasValue, ApplyConditionTo.CurrentValidator).WithMessage("Max price must be greater than or equal to min price.")
            .PrecisionScale(18, 2, false).WithMessage("Max price must not have more than 2 decimal places.");

        RuleFor(x => x.ProductName)
            .MinimumLength(2).WithMessage("Product name length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("Product name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Product name must only contain letters, digits, '-' and '_'.").When(x => !string.IsNullOrWhiteSpace(x.ProductName));

        RuleFor(x => x.ItemStatus)
            .MustBeAValidInventoryItemStatus().WithMessage("Invalid status value provided.");
    }
}