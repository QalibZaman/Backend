using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationErrorMessages.Customer.FirstNameRequiredErrorMessage)
            .Length(2, 30).WithMessage(string.Format(ValidationErrorMessages.Customer.FirstNameLengthErrorMessage, 2, 30))
            .Matches(@"^[A-Za-z]+(?: [A-Za-z]+)*$").WithMessage(ValidationErrorMessages.Customer.FirstNameIsInvalidErrorMessage)
            .Must(x => x == x.Trim()).WithMessage(ValidationErrorMessages.Customer.FirstNameStartsWithWhitespacesErrorMessage);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationErrorMessages.Customer.LastNameRequiredErrorMessage)
            .Length(2, 30).WithMessage(string.Format(ValidationErrorMessages.Customer.LastNameLengthErrorMessage, 2, 30))
            .Matches(@"^[A-Za-z]+(?: [A-Za-z]+)*$").WithMessage(
                ValidationErrorMessages.Customer.LastNameIsInvalidErrorMessage)
            .Must(x => x == x.Trim()).WithMessage(ValidationErrorMessages.Customer.LastNameStartsWithWhitespacesErrorMessage);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationErrorMessages.Customer.EmailRequiredErrorMessage)
            .NotNull().WithMessage(ValidationErrorMessages.Customer.EmailRequiredErrorMessage)
            .MaximumLength(256).WithMessage(ValidationErrorMessages.Customer.EmailLengthErrorMessage)
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$").WithMessage(ValidationErrorMessages.Customer.EmailInvalidErrorMessage);

        RuleFor(x => x.PhoneNumber)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.CountryId > 0)
            .WithMessage(ValidationErrorMessages.Customer.PhoneNumberRequiredErrorMessage);

        RuleFor(x => x.CountryId)
            .GreaterThan(0)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage(ValidationErrorMessages.Customer.CountryIdRequiredErrorMessage);

        RuleFor(x => x.Remarks)
            .Length(2, 100).WithMessage(string.Format(ValidationErrorMessages.Customer.RemarksLengthErrorMessage, 2, 100))
            .Matches(@"^[\x20-\x7E]+$").WithMessage(ValidationErrorMessages.Customer.RemarksAreInvalidErrorMessage)
            .When(x => !string.IsNullOrEmpty(x.Remarks));
    }
}
