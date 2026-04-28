using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateStaffRequestValidator : AbstractValidator<UpdateStaffRequest>
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 30;
    private const int MaxEmailLength = 256;
    public UpdateStaffRequestValidator()
    {
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x)
            .Must(IsAnyFieldProvided)
            .WithMessage(ValidationErrorMessages.Staff.AtLeastOneFieldErrorMessage);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MinimumLength(MinNameLength).WithMessage(string.Format(ValidationErrorMessages.Name.TooShortErrorMessage, MinNameLength))
            .MaximumLength(MaxNameLength).WithMessage(string.Format(ValidationErrorMessages.Name.TooLongErrorMessage, MaxNameLength))
            .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$")
                .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage)
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MinimumLength(MinNameLength).WithMessage(string.Format(ValidationErrorMessages.Name.TooShortErrorMessage, MinNameLength))
            .MaximumLength(MaxNameLength).WithMessage(string.Format(ValidationErrorMessages.Name.TooLongErrorMessage, MaxNameLength))
            .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$")
                .WithMessage(ValidationErrorMessages.Name.PatternErrorMessage)
            .When(x => x.LastName != null);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(MaxEmailLength).WithMessage(string.Format(ValidationErrorMessages.Email.TooLongErrorMessage, MaxEmailLength))
            .EmailAddress().WithMessage(ValidationErrorMessages.Email.PatternErrorMessage)
            .When(x => x.Email != null);

        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .When(x => x.RoleId.HasValue);

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage(ValidationErrorMessages.Staff.ActivationStatusRequired)
            .When(x => x.IsActive.HasValue);
    }

    private bool IsAnyFieldProvided(UpdateStaffRequest model)
    {
        return model.FirstName != null ||
               model.LastName != null ||
               model.Email != null ||
               model.RoleId != null ||
               model.IsActive.HasValue;
    }
}
