using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetCustomersRequestValidator : AbstractValidator<GetCustomersRequest>
{
    public GetCustomersRequestValidator()
    {
        RuleFor(x => x.Name)
            .Length(2, 30).WithMessage("Name length must be between 2 and 30 characters long.")
            .Matches(@"^[A-Za-z]+(?: [A-Za-z]+)*$")
            .WithMessage("Name must contain only Latin letters and single spaces between words.");

        RuleFor(x => x.Email)
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters long.")
            .Matches(@"^[A-Za-z0-9._+-]+@[A-Za-z0-9][A-Za-z0-9.-]*\.[A-Za-z.]{2,}$")
            .WithMessage("Email must be in a valid format.");

        RuleFor(x => x.LoyaltyId)
            .Length(6, 6).WithMessage("LoyaltyId length must be exactly 6 characters.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("LoyaltyId must contain only Latin letters and digits.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(30).WithMessage("Phone number length must be less than or equal to 30 characters.")
            .Matches(@"^(?=.*\d)\+?[0-9 ]+$").WithMessage("Phone number must contain only digits, spaces, and an optional leading plus.");

        RuleFor(x => x.Remarks)
            .MaximumLength(100).WithMessage("Remarks length must be less than or equal to 100 characters.")
            .Matches(@"^[\x20-\x7E]+$").WithMessage("Remarks must contain only Latin letters, digits, and allowed special characters.");
    }
}
