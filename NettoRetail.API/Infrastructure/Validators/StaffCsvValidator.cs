using FluentValidation;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class StaffCsvValidator : AbstractValidator<StaffCsvModel>
{
    public StaffCsvValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(ValidationErrorMessages.StaffCsv.FirstNameRequired)
            .MinimumLength(ValidationErrorMessages.StaffCsv.NameMinLength).WithMessage(ValidationErrorMessages.StaffCsv.FirstNameMinLength)
            .MaximumLength(ValidationErrorMessages.StaffCsv.NameMaxLength).WithMessage(ValidationErrorMessages.StaffCsv.FirstNameMaxLength)
            .Matches(ValidationErrorMessages.StaffCsv.NamePattern)
            .WithMessage(ValidationErrorMessages.StaffCsv.FirstNameInvalidFormat);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(ValidationErrorMessages.StaffCsv.LastNameRequired)
            .MinimumLength(ValidationErrorMessages.StaffCsv.NameMinLength).WithMessage(ValidationErrorMessages.StaffCsv.LastNameMinLength)
            .MaximumLength(ValidationErrorMessages.StaffCsv.NameMaxLength).WithMessage(ValidationErrorMessages.StaffCsv.LastNameMaxLength)
            .Matches(ValidationErrorMessages.StaffCsv.NamePattern)
            .WithMessage(ValidationErrorMessages.StaffCsv.LastNameInvalidFormat);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationErrorMessages.StaffCsv.EmailRequired)
            .MaximumLength(ValidationErrorMessages.StaffCsv.EmailMaxLength).WithMessage(ValidationErrorMessages.StaffCsv.EmailMaxLengthMessage)
            .Matches(ValidationErrorMessages.StaffCsv.EmailPattern)
            .WithMessage(ValidationErrorMessages.StaffCsv.EmailInvalidFormat);

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage(ValidationErrorMessages.StaffCsv.RoleRequired)
            .Must(role => int.TryParse(role, out var roleId) && roleId > 0)
            .WithMessage(ValidationErrorMessages.StaffCsv.RoleInvalid);
    }
}
