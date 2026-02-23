using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemModel>
{
    public UpdateInventoryItemValidator()
    {
        RuleFor(x => x.ProductUpc)
            .NotEmpty().WithMessage("Product UPC must be exactly 12 digits long.")
            .Matches(@"^\d{12}$").WithMessage("Product UPC must be exactly 12 digits long.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Invalid item name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid item name. Please check format or length.")
            .MaximumLength(50).WithMessage("Invalid item name. Please check format or length.")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Invalid item name. Please check format or length.");

        RuleFor(x => x.CategoryId)
            .NotNull().WithMessage("Please select a category.")
            .GreaterThan(0).WithMessage("Please select a category.");

        RuleFor(x => x.BrandId)
            .NotNull().WithMessage("Product Brand is required.")
            .GreaterThan(0).WithMessage("Product Brand is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Invalid description. Please check format or length.")
            .MaximumLength(500).WithMessage("Invalid description. Please check format or length.")
            .Matches(@"^[A-Za-z0-9 ,.\-_'()]*$").WithMessage("Invalid description. Please check format or length.")
            .Matches(@"^(?! ).*(?<! )$").WithMessage("Invalid description. Please check format or length.")
            .Matches(@"^(?!.*<[^>]+>).*$").WithMessage("Invalid description. Please check format or length.");

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive is required.");

        RuleFor(x => x.Price)
            .NotNull().WithMessage("Price is required.")
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .Must(p => p == null || decimal.Round(p.Value, 2) == p.Value)
            .WithMessage("Price must not have more than two decimal places.");

        RuleFor(x => x.IsPromoted)
            .NotNull().WithMessage("IsPromoted is required.");

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.IsPromoted != true)
                    return x.PromotionPrice is null;

                return x.PromotionPrice.HasValue
                       && x.Price.HasValue
                       && x.PromotionPrice.Value < x.Price.Value;
            })
            .WithMessage("Promotion price must be less than the regular price when promoted.");
    }
}
