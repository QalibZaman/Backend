using FluentValidation;
using NettoRetail.Application.Models.Request.Password;

namespace NettoRetail.API.Infrastructure.Validators;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Matches(@"^\S+@\S+\.\S+$").WithMessage("Email must not contain spaces.")
            .Matches(@"^(?!.*\.\.).*$").WithMessage("Email must not contain consecutive dots.");
    }
}
