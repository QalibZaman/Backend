using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetFilteredStaffRequestValidator : AbstractValidator<GetFilteredStaffRequest>
{

    private readonly IValidator<SortingOption> _validator;

    public GetFilteredStaffRequestValidator(IValidator<SortingOption> validator)
    {
        _validator = validator;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage)
            .MaximumLength(30).WithMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage)
            .Matches("^[A-Za-z]+$").WithMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage)
            .Matches("^[A-Z][a-z]*$").WithMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage)
            .Matches(@"^\S.*\S$|^\S$").WithMessage(ValidationErrorMessages.Staff.InvalidFirstNameErrorMessage);

        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage)
            .MaximumLength(30).WithMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage)
            .Matches("^[A-Za-z]+$").WithMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage)
            .Matches("^[A-Z][a-z]*$").WithMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage)
            .Matches(@"^\S.*\S$|^\S$").WithMessage(ValidationErrorMessages.Staff.InvalidLastNameErrorMessage);

        RuleFor(x => x.Email)
            .MaximumLength(256)
            .EmailAddress().WithMessage(ValidationErrorMessages.Email.PatternErrorMessage);

        RuleFor(x => x.Status)
            .MustBeAValidAccountStatus().WithMessage(ValidationErrorMessages.Staff.InvalidStatusErrorMessage);

        RuleForEach(x => x.SortingOptions)
            .SetValidator(_validator);
    }
}
