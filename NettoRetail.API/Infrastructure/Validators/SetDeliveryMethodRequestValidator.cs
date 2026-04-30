using FluentValidation;
using NettoRetail.Application.Models.Delivery;

namespace NettoRetail.API.Infrastructure.Validators;

public class SetDeliveryMethodRequestValidator : AbstractValidator<SetDeliveryMethodRequest>
{
    public SetDeliveryMethodRequestValidator()
    {
        RuleFor(x => x.DeliveryType)
            .NotNull().WithMessage("Please select a delivery method.")
            .IsInEnum().WithMessage("Please select a delivery method.");
    }
}
