using FluentValidation;
using NettoRetail.Application.CQRS.Payments.Commands;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class AddPaymentCommandValidator : AbstractValidator<AddPaymentCommand>
    {
        public AddPaymentCommandValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage(ValidationErrorMessages.Payment.IdNotEmpty);

            RuleFor(model => model.PaymentMethodId)
                .NotEmpty().WithMessage(ValidationErrorMessages.Payment.TokenNotEmpty)
                .Must(x => x.StartsWith("pm_")).WithMessage(ValidationErrorMessages.Payment.InvalidPaymentMethodIdFormat);
        }
    }
}
