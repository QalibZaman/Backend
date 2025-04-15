using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateInventoryItemValidator : AbstractValidator<UpdateInventoryItemModel>
{
    public UpdateInventoryItemValidator()
    {
        RuleFor(x => x)
            .Must(model =>
                model.ProductName != null ||
                model.CategoryId != null ||
                model.ProductUpc != null ||
                model.Description != null ||
                model.IsActive.HasValue ||
                model.AvailableAmount.HasValue ||
                model.ReservedAmount.HasValue ||
                model.OnHandAmount.HasValue ||
                model.Price.HasValue ||
                model.PromotionPrice.HasValue ||
                model.IsPromoted.HasValue)
            .WithMessage("At least one field must be provided to update the inventory item.")
            .Must(model => model.ReservedAmount <= model.AvailableAmount)
            .WithMessage("Reserved amount cannot exceed the available amount.")
            .Must(model => model.OnHandAmount >= model.ReservedAmount + model.AvailableAmount)
            .WithMessage("The sum of Reserved Amount and Available Amount should not exceed the On-hand Amount.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MinimumLength(2).WithMessage("Product name length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("Product name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Product name must only contain letters, digits, '-' and '_'")
            .When(x => x.ProductName != null);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category id must be greater than 0.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.ProductUpc)
            .Must(upc => upc.HasValue && upc.Value.ToString().Length == 12)
            .WithMessage("Product UPC must be exactly 12 digits long.")
            .When(x => x.ProductUpc.HasValue);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must be less than 500 characters long.")
            .Matches(@"^[A-Za-z0-9 ,.\-_'()]*$").WithMessage("Description must only contain letters, digits, spaces, and common punctuation.")
            .Matches("^(|[^ ].*[^ ]$)").WithMessage("Description must not have leading or trailing spaces.")
            .When(x => x.Description != null);

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive must be a valid boolean value.")
            .When(x => x.IsActive.HasValue);

        RuleFor(x => x.AvailableAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Available amount must be 0 or greater.")
            .When(x => x.AvailableAmount.HasValue);

        RuleFor(x => x.ReservedAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Reserved amount must be 0 or greater.")
            .When(x => x.ReservedAmount.HasValue);

        RuleFor(x => x.OnHandAmount)
            .GreaterThanOrEqualTo(0).WithMessage("On-hand amount must be 0 or greater.")
            .When(x => x.OnHandAmount.HasValue);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .Must(p => decimal.Round(p.Value, 2) == p.Value)
            .WithMessage("Price must not have more than two decimal places.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.PromotionPrice)
            .GreaterThan(0).WithMessage("Promotion price must be greater than 0.")
            .When(x => x.PromotionPrice.HasValue);

        RuleFor(x => x)
            .Must(x => x.Price.HasValue && x.PromotionPrice.HasValue && x.PromotionPrice < x.Price)
            .WithMessage("Promotion price must be less than the regular price.")
            .When(x => x.Price.HasValue && x.PromotionPrice.HasValue);

        RuleFor(x => x.IsPromoted)
            .NotNull().WithMessage("IsPromoted must be a valid boolean value.")
            .When(x => x.IsPromoted.HasValue);
    }
}
