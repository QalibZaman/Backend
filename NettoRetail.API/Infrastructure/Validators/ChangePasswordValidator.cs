using FluentValidation;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.")
            .NotNull().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
           .NotEmpty().WithMessage("Password is required.")
           .NotNull().WithMessage("Password is required.")
           .MinimumLength(8).WithMessage(ValidationErrorMessages.Password.PasswordSize)
           .MaximumLength(20).WithMessage(ValidationErrorMessages.Password.PasswordMaxSize)
           .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
           .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
           .Matches("[0-9]").WithMessage("Password must contain at least one numeric character.")
           .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least one special character.")
           .Matches(@"^\S*$").WithMessage("Password must not contain spaces.");

        RuleFor(x => x.ConfirmPassword)
           .NotEmpty().WithMessage("Confirmation password is required.")
           .NotNull().WithMessage("Confirmation password is required.")
           .Equal(x => x.NewPassword).WithMessage(ValidationErrorMessages.Password.PasswordConfirmationMismatchErrorMessage);
    }
}
