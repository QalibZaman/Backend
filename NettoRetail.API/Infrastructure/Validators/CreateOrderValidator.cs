using FluentValidation;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderModel>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");
        
        RuleFor(x => x.DeliveryType)
            .IsInEnum().WithMessage("DeliveryType must be a valid type.");
        
        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Products data is required.");

        RuleFor(x => x.Products)
            .Must(products => products.Select(p => p.InventoryItemId).Distinct().Count() == products.Count)
            .When(x => x.Products is { Count: > 0 })
            .WithMessage("Products must contain unique inventory item IDs.");

        RuleForEach(x => x.Products).ChildRules(product =>
        {
            product.RuleFor(p => p.InventoryItemId).NotEmpty()
                .WithMessage("Product ID must not be empty.");

            product.RuleFor(p => p.Amount).GreaterThan(0)
                .WithMessage("Product quantity must be greater than 0.");
        });
    }
}
