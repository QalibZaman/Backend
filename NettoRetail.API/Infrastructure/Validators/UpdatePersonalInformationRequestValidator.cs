using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdatePersonalInformationRequestValidator : AbstractValidator<UpdatePersonalInformationRequest>
    {
        public UpdatePersonalInformationRequestValidator()
        {
            RuleFor(x => x.CountryId)
           .NotEmpty().WithMessage("CountryId is required.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Length(7, 14).WithMessage("Incorrect phone number length");

            RuleFor(x => x.ZipCode)
                .NotEmpty().WithMessage("Zipcode is required.")
                .Length(3, 8).WithMessage("Incorrect zipcode length");

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

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Address is required.")
                .Length(5, 90).WithMessage("Address must be between 5 and 90 characters.")
                .Matches(@"^[A-Za-z0-9\s\-,.]+$").WithMessage("Address can only contain Latin characters, numbers, and special characters.")
                .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .Length(2, 50).WithMessage("City must be between 2 and 50 characters.")
                .Matches(@"^[A-Za-z\s-]+$").WithMessage("City can only contain Latin characters, spaces, and hyphens.")
                .Must(x => x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.");

            RuleFor(x => x.State)
                .Length(2, 50).WithMessage("State must be between 2 and 50 characters.")
                .Matches(@"^[A-Za-z0-9\s\-,.]+$").WithMessage("State can only contain Latin characters, numbers, spaces, and special characters.")
                .Must(x => x == null || x.Trim() == x).WithMessage("No leading or trailing spaces are allowed.");
        }
    }
}
