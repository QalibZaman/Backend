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
            .NotEmpty().WithMessage("EM-54")
            .MinimumLength(8).WithMessage("EM-54")
            .MaximumLength(20).WithMessage("EM-54")
            .Matches("[A-Z]").WithMessage("EM-54")
            .Matches("[a-z]").WithMessage("EM-54")
            .Matches("[0-9]").WithMessage("EM-54")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("EM-54")
            .Matches(@"^\S*$").WithMessage("EM-54");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("EM-53")
            .Equal(x => x.Password)
            .WithMessage("EM-53");
    }
}
