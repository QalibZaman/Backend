using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 30).WithMessage("First name length must be between 2 and 30 characters long.")
            .Matches("^[A-Za-z]+$").WithMessage("First name must contain only Latin characters.")
            .Matches("^[A-Z][a-z]*$").WithMessage("First name must start with a single uppercase letter followed by lowercase letters only.")
            .Matches(@"^[^\s].*[^\s]$").WithMessage("First name must not start or end with spaces.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 30).WithMessage("Last name length must be between 2 and 30 characters long.")
            .Matches("^[A-Za-z]+$").WithMessage("Last name must contain only Latin characters.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Last name must start with a single uppercase letter followed by lowercase letters only.")
            .Matches(@"^[^\s].*[^\s]$").WithMessage("Last name must not start or end with spaces.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters long.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$").WithMessage("Email must be in a valid format.");

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

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmation password is required.")
            .NotNull().WithMessage("Confirmation password is required.")
            .Equal(x => x.Password).WithMessage("Passwords aren't matching.");
    }
}
