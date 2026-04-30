using FluentValidation;
using NettoRetail.Application.Models.Delivery;

namespace NettoRetail.API.Infrastructure.Validators;

public class SetDeliveryAddressRequestValidator : AbstractValidator<SetDeliveryAddressRequest>
{
    public SetDeliveryAddressRequestValidator()
    {
        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Please select a delivery address.");
    }
}
