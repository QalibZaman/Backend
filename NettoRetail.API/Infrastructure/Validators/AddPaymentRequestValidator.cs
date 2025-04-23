using FluentValidation;
using NettoRetail.Application.Models.Payments;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class AddPaymentRequestValidator : AbstractValidator<AddPaymentRequest>
    {
        public AddPaymentRequestValidator()
        {
            RuleFor(model => model.Token)
                .NotNull().WithMessage("Token cannot be null")
                .NotEmpty().WithMessage("Token cannot be empty");
        }
    }
}
