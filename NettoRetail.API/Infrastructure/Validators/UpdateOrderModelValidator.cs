using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateOrderModelValidator : AbstractValidator<UpdateOrderModel>
{
    public UpdateOrderModelValidator()
    {
        RuleFor(x => x.DeliveryType)
            .MustBeAValidDeliveryType().WithMessage("Delivery type must be valid");

        RuleFor(x => x.ItemsToUpsert)
            .Must(dict => dict != null && dict.All(kv => kv.Value > 0))
            .WithMessage("All item quantities must be greater than 0.");
    }
}
