using FluentValidation;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateOrderModelValidator : AbstractValidator<UpdateOrderModel>
{
    public UpdateOrderModelValidator()
    {
        RuleFor(x => x.ItemsToUpsert)
            .Must(dict => dict.Values.All(x => x > 0))
            .WithMessage(ValidationErrorMessages.Order.ItemGreaterThanZeroErrorMessage)
            .Must((model, upsertDict) => !upsertDict.Keys.Any(x => model.ItemsToDelete.Contains(x)))
            .WithMessage(ValidationErrorMessages.Order.ItemUpsertDeleteConflictErrorMessage)
            .When(x => x.ItemsToDelete != null);

        RuleFor(x => x.DeliveryType)
            .IsInEnum()
            .WithMessage(ValidationErrorMessages.Order.InvalidDeliveryErrorMessage);
    }
}
