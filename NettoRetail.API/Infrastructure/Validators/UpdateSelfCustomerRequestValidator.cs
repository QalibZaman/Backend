using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateSelfCustomerRequestValidator : AbstractValidator<UpdateSelfCustomerRequest>
    {
        public UpdateSelfCustomerRequestValidator()
        {
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleFor(x => x.Email)
                .MinimumLength(2)
                .MaximumLength(256)
                .EmailAddress();

            RuleFor(x => x.FirstName)
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(x => x == x?.Trim()).WithMessage("Leading or trailing spaces are not allowed")
                .Matches(@"^[a-zA-Z]+( [a-zA-Z]+)*$").WithMessage("First name must contain only Latin letters and single internal spaces");

            RuleFor(x => x.LastName)
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(x => x == x?.Trim()).WithMessage("Leading or trailing spaces are not allowed")
                .Matches(@"^[a-zA-Z]+( [a-zA-Z]+)*$").WithMessage("Last name must contain only Latin letters and single internal spaces");

            RuleFor(customer => customer.PhoneNumber)
                .NotEmpty()
                .Matches(@"^\d+( \d+)*$").WithMessage("Phone number must contain only digits.");
        }
    }
}
