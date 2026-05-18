using FluentValidation;
using NettoRetail.Application.Models.Delivery;
using NettoRetail.Domain.Enums;

namespace NettoRetail.API.Infrastructure.Validators;

public class SetDeliveryMethodRequestValidator : AbstractValidator<SetDeliveryMethodRequest>
{
    public SetDeliveryMethodRequestValidator()
    {
        RuleFor(x => x.DeliveryType)
            .NotNull().WithMessage("Please select a delivery method.")
            .Must(x => x == OrderDeliveryTypeEnum.Courier || x == OrderDeliveryTypeEnum.InShop)
            .WithMessage("Please select a delivery method.");
    }
}
