using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class SendResetPasswordLinkRequesValidator : AbstractValidator<SendResetPasswordLinkRequest>
    {
        public SendResetPasswordLinkRequesValidator()
            {
                RuleFor(x => x.Email)
                    .NotEmpty()
                    .WithMessage("Email is required.")
                    .NotNull()
                    .WithMessage("Email is required.")
                    .MaximumLength(256)
                    .WithMessage("Email length must be less than 256 characters long.")
                    .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$")
                    .WithMessage("Email must be in a valid format.");
        }
    }
}
