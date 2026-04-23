using FluentValidation;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.CQRS.Payments.Queries;

public class GetPaymentMethodsQueryValidator : AbstractValidator<GetPaymentMethodsQuery>
{
    public GetPaymentMethodsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(ValidationErrorMessages.Payment.IdNotEmpty);

        RuleFor(x => x.ShopId)
            .GreaterThan(0)
            .WithMessage(ValidationErrorMessages.Shop.ShopIdRequiredErrorMessage);
    }
}
