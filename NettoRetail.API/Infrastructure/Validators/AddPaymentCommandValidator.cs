using FluentValidation;
using NettoRetail.Application.CQRS.Payments.Commands;

namespace NettoRetail.API.Infrastructure.Validators;

public class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
{
    public AddPaymentCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account id cannot be empty.");

        RuleFor(x => x.ShopId)
            .GreaterThan(0)
            .WithMessage("Shop id must be greater than 0.");

        RuleFor(x => x.PaymentMethodId)
            .NotEmpty()
            .WithMessage("Payment method token cannot be empty.")
            .Must(x => x.StartsWith("pm_", StringComparison.Ordinal))
            .WithMessage("Invalid payment method id format.");
    }
}