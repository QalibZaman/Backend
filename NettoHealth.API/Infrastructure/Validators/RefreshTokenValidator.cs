using FluentValidation;
using NettoHealth.Application.Models.Request;

namespace NettoHealth.API.Infrastructure.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotEmpty();
    }
}