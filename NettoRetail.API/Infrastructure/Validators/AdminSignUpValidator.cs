using FluentValidation;
using NettoRetail.Application.Models.Admin;

namespace NettoRetail.API.Infrastructure.Validators;

public class AdminSignUpValidator : AbstractValidator<AdminSignUpModel>
{
    public AdminSignUpValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Invalid First Name. Please check format or length.")
            .NotNull().WithMessage("Invalid First Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid First Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches(@"^\S.*\S$|^\S$").WithMessage("Invalid First Name. Please check format or length.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Invalid Last Name. Please check format or length.")
            .NotNull().WithMessage("Invalid Last Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid Last Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid Last Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid Last Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid Last Name. Please check format or length.")
            .Matches(@"^\S.*\S$|^\S$").WithMessage("Invalid Last Name. Please check format or length.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters long.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$").WithMessage("Email must be in a valid format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .NotNull().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be greater than 8 characters long.")
            .MaximumLength(20).WithMessage("Password must be less than 20 characters long.")
            .Equal(x => x.ConfirmPassword).WithMessage("Passwords aren't matching.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain at least one numeric character.")
            .Matches(@"[!""#$%&'()*+,-./:;<=>?@[\\\]^_`{|}~]").WithMessage("Password must contain at least one special character.")
            .Matches(@"^\S*$").WithMessage("Password must not contain spaces.");

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
    }
}