using FluentValidation;
using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddInventoryItemValidator : AbstractValidator<AddInventoryItemModel>
{
    public AddInventoryItemValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .NotNull().WithMessage("Product name is required.")
            .MinimumLength(2).WithMessage("Product name length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("Product name length must be less than 50 characters long.")
            .Matches("^[a-zA-Z0-9-_ ]+$").WithMessage("Product name must only contain letters, digits, spaces, '-' and '_'.");

        RuleFor(x => x.CategoryId).GreaterThan(0);

        RuleFor(x => x.BrandId).GreaterThan(0);

        RuleFor(x => x.ProductUpc)
            .NotEmpty().WithMessage("Product UPC is required.")
            .Matches(@"^\d{12}$").WithMessage("Product UPC must be exactly 12 digits long.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be greater or equal to 0.")
            .PrecisionScale(18, 2, false).WithMessage("Price must not have more than 2 decimal places.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters long.")
            .Matches(@"^[A-Za-z0-9 ,.\-_'()]*$")
            .WithMessage("Description must only contain letters, digits, spaces, and common punctuation.")
            .Matches("^(|[^ ].*[^ ]$)").WithMessage("Description must not have leading or trailing spaces.");

        RuleForEach(x => x.ProductDetails).SetValidator(new ProductDetailsModelValidator());
    }
}
