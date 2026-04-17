using FluentValidation;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class ChangeDefaultPaymentCommandValidator : AbstractValidator<ChangeDefaultPaymentCommand>
{
    public ChangeDefaultPaymentCommandValidator()
    {
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty();

        RuleFor(x => x.StripePaymentMethodId)
            .Must(MatchesStripePaymentMethodId)
            .WithMessage(ValidationErrorMessages.Payment.InvalidPaymentMethodIdFormat);
    }

    private bool MatchesStripePaymentMethodId(string paymentMethodId)
    {
        return !string.IsNullOrEmpty(paymentMethodId)
            && paymentMethodId.StartsWith("pm_")
                && paymentMethodId.Length > 3;
    }
}
