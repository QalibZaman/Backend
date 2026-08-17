namespace NettoRetail.API.Infrastructure.Validators;

using FluentValidation;
using NettoRetail.Application.Models.Admin;

public class AdminSignUpModelValidator : AbstractValidator<AdminSignUpModel>
{
    public AdminSignUpModelValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Invalid First Name. Please check format or length.")
            .NotNull().WithMessage("Invalid First Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid First Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid First Name. Please check format or length.")
            .Matches(@"^[A-Z][a-zA-Z]*(?:\s[A-Z][a-zA-Z]*)*$").WithMessage("Invalid First Name. Please check format or length.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Invalid Last Name. Please check format or length.")
            .NotNull().WithMessage("Invalid Last Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid Last Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid Last Name. Please check format or length.")
            .Matches(@"^[A-Z][a-zA-Z]*(?:\s[A-Z][a-zA-Z]*)*$").WithMessage("Invalid Last Name. Please check format or length.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Invalid Email. Please check format or length.")
            .NotNull().WithMessage("Invalid Email. Please check format or length.")
            .MaximumLength(256).WithMessage("Invalid Email. Please check format or length.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$")
            .WithMessage("Invalid Email. Please check format or length.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .NotNull().WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage(
                "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.")
            .MaximumLength(20)
            .WithMessage(
                "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.")
            .Equal(x => x.ConfirmPassword).WithMessage("\t\nPasswords do not match. Please try again.")
            .Matches("[A-Z]")
            .WithMessage(
                "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.")
            .Matches("[a-z]")
            .WithMessage(
                "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.")
            .Matches("[0-9]")
            .WithMessage(
                "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage(
                "Password must contain between 8-20 characters, include at least one uppercase letter, one lowercase letter, one number and one special character.");
    }
}