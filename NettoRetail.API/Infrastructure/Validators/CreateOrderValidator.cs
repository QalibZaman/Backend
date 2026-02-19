using System.Data;
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
            .NotEmpty().WithMessage("DeliveryType is required.")
            .IsInEnum().WithMessage("DeliveryType must be a valid type.");
        
        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Products data is required.")
            .Must(x => x.Keys.All(k => k != Guid.Empty)).WithMessage("Product IDs must not be empty.")
            .Must(x => x.Values.All(v => v > 0)).WithMessage("Product quantities must be greater than 0.");
    }
}