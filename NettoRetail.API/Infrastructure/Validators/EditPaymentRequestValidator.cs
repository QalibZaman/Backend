using FluentValidation;
using NettoRetail.Application.Models.Payments;

namespace NettoRetail.API.Infrastructure.Validators;

public class EditPaymentRequestValodator : AbstractValidator<EditPaymentRequest>
{
    public EditPaymentRequestValodator()
    {
        RuleFor(x => x.StripePaymentMethodId)
            .NotEmpty().WithMessage("Payment method ID is required.");

        RuleFor(x => x.CardHolderName)
            .NotEmpty().WithMessage("Cardholder name is required.")
            .MaximumLength(100);
    }
}
