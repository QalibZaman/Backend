using FluentValidation;
using NettoRetail.Application.Models.Authentication;
namespace NettoRetail.API.Infrastructure.Validators;
public class LoginValidator : AbstractValidator<LoginModel>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address. Please check and try again.");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,20}$")
            .WithMessage("Password must be 8-20 characters long, include at least 1 uppercase letter, 1 lowercase letter, 1 digit, and 1 special character.");
    }
}