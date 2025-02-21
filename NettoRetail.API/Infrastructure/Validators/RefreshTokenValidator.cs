using FluentValidation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotEmpty();
    }
}