using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateStaffValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.FirstName)
             .NotEmpty().WithMessage(ValidationErrorMessages.Account.FirstNameRequiredErrorMessage)
             .MinimumLength(2)
             .MaximumLength(30)
             .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$")
             .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationErrorMessages.Account.LastNameRequiredErrorMessage)
            .MinimumLength(2)
            .MaximumLength(30)
            .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$")
            .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationErrorMessages.Email.EmailRequiredErrorMessage)
            .MaximumLength(256)
            .EmailAddress()
            .WithMessage(ValidationErrorMessages.Email.PatternErrorMessage);

        RuleFor(x => x.RoleId)
                .GreaterThan(0);
    }
}
