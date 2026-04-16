using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateCustomerRequestValidator : AbstractValidator<UpdateCustomerRequest>
{
    public UpdateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 30).WithMessage("First name length must be between 2 and 30 characters long.")
            .Matches(@"^[A-Za-z]+(?: [A-Za-z]+)*$").WithMessage(
                "First name must contain only Latin letters and single spaces between words.")
            .Must(x => x == x.Trim()).WithMessage("First name must not start or end with spaces.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 30).WithMessage("Last name length must be between 2 and 30 characters long.")
            .Matches(@"^[A-Za-z]+(?: [A-Za-z]+)*$").WithMessage(
                "Last name must contain only Latin letters and single spaces between words.")
            .Must(x => x == x.Trim()).WithMessage("Last name must not start or end with spaces.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters long.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$")
            .WithMessage("Email must be in a valid format.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
                .When(x => x.CountryId > 0)
                .WithMessage("Phone number can't be empty when CountryId is provided.")
            .Matches(@"^(?=.*\d)[0-9 ]+$")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Phone number must contain digits and spaces only.")
            .Length(4, 14)
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Phone number length must be between 4 and 14 digits.")
            .Must(x => x == x.Trim())
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Phone number must not start or end with spaces.");

        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("CountryId is required when PhoneNumber is provided.");

        RuleFor(x => x.Remarks)
            .Length(2, 100)
            .When(x => x.Remarks is not null)
            .WithMessage("Remarks length must be between 2 and 100 characters.")
            .Matches(@"^[\x20-\x7E]+$")
            .When(x => x.Remarks is not null)
            .WithMessage("Remarks must contain only Latin letters, digits, and allowed special characters.");
    }
}
