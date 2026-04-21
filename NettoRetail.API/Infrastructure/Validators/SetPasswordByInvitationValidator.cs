using System.Data;
using FluentValidation;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class SetPasswordByInvitationValidator : AbstractValidator<SetPasswordByInvitationRequest>
{
    public SetPasswordByInvitationValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Your invitation link is invalid or expired.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("")
            .MinimumLength(8).WithMessage(ValidationErrorMessages.Password.PasswordSize)
            .MaximumLength(20).WithMessage(ValidationErrorMessages.Password.PasswordSize)
            .Matches("[A-Z]").WithMessage(ValidationErrorMessages.Password.PasswordSize)
            .Matches("[a-z]").WithMessage(ValidationErrorMessages.Password.PasswordSize)
            .Matches("[0-9]").WithMessage(ValidationErrorMessages.Password.PasswordSize)
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage(ValidationErrorMessages.Password.PasswordSize)
            .Matches(@"^\S*$").WithMessage(ValidationErrorMessages.Password.PasswordSize);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("New password and confirmation password do not match. Please check your input.")
            .Equal(x => x.Password)
            .WithMessage("New password and confirmation password do not match. Please check your input.");
    }
}
