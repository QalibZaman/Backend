using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

using FluentValidation;
using NettoRetail.Application.Models.Admin;

public class AdminSignUpModelValidator : AbstractValidator<AdminSignUpModel>
{
    public AdminSignUpModelValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)
            .NotNull().WithMessage(ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)
            .MinimumLength(2).WithMessage(ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)
            .MaximumLength(30).WithMessage(ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage)
            .Matches(@"^[A-Z][a-zA-Z]*(?:\s[A-Z][a-zA-Z]*)*$").WithMessage(ValidationErrorMessages.Staff.FirstNameIsInvalidErrorMessage);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)
            .NotNull().WithMessage(ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)
            .MinimumLength(2).WithMessage(ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)
            .MaximumLength(30).WithMessage(ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage)
            .Matches(@"^[A-Z][a-zA-Z]*(?:\s[A-Z][a-zA-Z]*)*$").WithMessage(ValidationErrorMessages.Staff.LastNameIsInvalidErrorMessage);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage)
            .NotNull().WithMessage(ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage)
            .MaximumLength(256).WithMessage(ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage)
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$")
            .WithMessage(ValidationErrorMessages.Staff.EmailIsInvalidErrorMessage);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationErrorMessages.Staff.PasswordIsRequiredErrorMessage)
            .NotNull().WithMessage(ValidationErrorMessages.Staff.PasswordIsRequiredErrorMessage)
            .MinimumLength(8)
            .WithMessage(
                ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)
            .MaximumLength(20)
            .WithMessage(
                ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)
            .Matches("[A-Z]")
            .WithMessage(
                ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)
            .Matches("[a-z]")
            .WithMessage(
                ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)
            .Matches("[0-9]")
            .WithMessage(
                ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage)
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage(
                ValidationErrorMessages.Staff.PasswordIsInvalidErrorMessage);
    }
}