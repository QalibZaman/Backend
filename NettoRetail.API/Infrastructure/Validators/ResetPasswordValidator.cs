using FluentValidation;
using NettoRetail.Application.Models.Request.Password;

namespace NettoRetail.API.Infrastructure.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.")
            .NotNull().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
           .NotEmpty().WithMessage("Password is required.")
            .NotNull().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(20).WithMessage("Password must be less than 20 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain at least one numeric character.")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least one special character.")
            .Matches(@"^\S*$").WithMessage("Password must not contain spaces.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmation password is required.")
            .NotNull().WithMessage("Confirmation password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords aren't matching.");
    }
}
