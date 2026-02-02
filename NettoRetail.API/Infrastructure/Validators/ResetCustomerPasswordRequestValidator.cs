using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class ResetCustomerPasswordRequestValidator : AbstractValidator<ResetCustomerPasswordRequest>
    {
        public ResetCustomerPasswordRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotNull().WithMessage("Token is required.")
                .NotEmpty().WithMessage("Token is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .NotNull().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .MaximumLength(20).WithMessage("Password must be less than 20 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
                .Matches("[0-9]").WithMessage("Password must contain at least one numeric character.")
                .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least one special character.")
                .Matches(@"^\S*$").WithMessage("Password must not contain spaces.");

            RuleFor(x => x.PasswordConfirmation)
                .NotEmpty().WithMessage("Confirmation password is required.")
                .NotNull().WithMessage("Confirmation password is required.")
                .Equal(x => x.Password).WithMessage("Passwords aren't matching.");
        }
    }
}
