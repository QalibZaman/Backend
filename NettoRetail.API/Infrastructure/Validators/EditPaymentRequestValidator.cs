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

        RuleFor(x => x.ExpMonth)
            .InclusiveBetween(1, 12)
            .WithMessage("Expiration month must be between 1 and 12.");

        RuleFor(x => x.ExpYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year)
            .WithMessage("Expiration year must be this year or later.");

        RuleFor(x => x)
        .Must(x =>
        {
            int year = (int)x.ExpYear;
            int month = (int)x.ExpMonth;
            return new DateTime(year, month, 1) >= DateTime.UtcNow;
        })
        .WithMessage("The expiration date must be in the future.");

    }
}
