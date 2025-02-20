using FluentValidation;
using NettoHealth.Application.Models.Entities;

namespace NettoHealth.API.Infrastructure.Validators;

public class HealthcareProviderRegistrationValidator : AbstractValidator<HealthcareProviderRegistrationModel>
{
    public HealthcareProviderRegistrationValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .NotNull().WithMessage("First name is required.")
            .Matches("^[A-Za-z]+$").WithMessage("First name must contain only Latin characters.")
            .MinimumLength(2).WithMessage("First name length must be greater than 2 characters long.")
            .MaximumLength(30).WithMessage("First name length must be less than 30 characters long.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .NotNull().WithMessage("Last name is required.")
            .Matches("^[A-Za-z]+$").WithMessage("Last name must contain only Latin characters.")
            .MinimumLength(2).WithMessage("Last name length must be greater than 2 characters long.")
            .MaximumLength(30).WithMessage("Last name length must be less than 30 characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email length must be less than 100 characters long.")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .NotNull().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be greater than 8 characters long.")
            .MaximumLength(20).WithMessage("Password must be less than 20 characters long.")
            .Equal(x => x.ConfirmPassword).WithMessage("Passwords aren't matching.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain at least one numeric character.")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password is required.")
            .NotNull().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be greater than 8 characters long.")
            .MaximumLength(20).WithMessage("Password must be less than 20 characters long.")
            .Equal(x => x.Password).WithMessage("Passwords aren't matching.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain at least one numeric character.")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Gender must be a valid value.");

        RuleFor(x => x.Subscription)
            .IsInEnum().WithMessage("Subscription must be a valid value.");

        RuleFor(x => x.Speciality)
            .IsInEnum().WithMessage("Speciality must be a valid value.");
    }
}