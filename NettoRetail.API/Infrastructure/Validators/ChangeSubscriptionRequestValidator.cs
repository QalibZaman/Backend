using FluentValidation;
using NettoRetail.Application.Models.Request.Subscription;

namespace NettoRetail.API.Infrastructure.Validators;

public class ChangeSubscriptionRequestValidator : AbstractValidator<ChangeSubscriptionRequest>
{
    public ChangeSubscriptionRequestValidator()
    {
        RuleFor(x => x.PaymentMethodId)
            .Must(v => !string.IsNullOrWhiteSpace(v))
                .WithMessage("Payment method Id is required.");

        RuleFor(x => x.PaymentMethodId)
            .Matches("^pm_[a-zA-Z0-9]+$")
            .When(s => !string.IsNullOrWhiteSpace(s.PaymentMethodId))
                .WithMessage("Invalid payment method id format.");

        RuleFor(x => x.NewPlan)
            .NotNull()
                .WithMessage("The Plan should be attached")
            .IsInEnum();
    }
}
