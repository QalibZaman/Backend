using System.Data;
using FluentValidation;
using NettoRetail.Application.Models.Request.Password;

namespace NettoRetail.API.Infrastructure.Validators;

public class SetPasswordByInvitationValidator : AbstractValidator<SetPasswordByInvitationRequest>
{
    public SetPasswordByInvitationValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Your invitation link is invalid or expired.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password must contain at least 8 characters")
            .MinimumLength(8).WithMessage("Password must contain at least 8 characters")
            .MaximumLength(20).WithMessage("Password must contain at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least 8 characters")
            .Matches("[a-z]").WithMessage("Password must contain at least 8 characters")
            .Matches("[0-9]").WithMessage("Password must contain at least 8 characters")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least 8 characters")
            .Matches(@"^\S*$").WithMessage("Password must contain at least 8 characters");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("New password and confirmation password do not match. Please check your input.")
            .Equal(x => x.Password)
            .WithMessage("New password and confirmation password do not match. Please check your input.");
    }
}
