using FluentValidation;
using NettoRetail.Application.Models.Authentication;
namespace NettoRetail.API.Infrastructure.Validators;
public class LoginValidator : AbstractValidator<LoginModel>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("Email is required.");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}