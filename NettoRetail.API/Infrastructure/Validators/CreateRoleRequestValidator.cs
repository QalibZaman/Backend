using FluentValidation;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty()
               .Length(ValidationErrorMessages.Role.NameMinLength, ValidationErrorMessages.Role.NameMaxLength)
               .WithMessage(ValidationErrorMessages.Role.NameLengthErrorMessage)
               .Matches(ValidationErrorMessages.Role.NameAllowedCharactersPattern)
               .WithMessage(ValidationErrorMessages.Role.NameAllowedCharactersRegex);

            RuleFor(x => x.Description)
               .MaximumLength(ValidationErrorMessages.Role.DescriptionMaxLength).WithMessage(ValidationErrorMessages.Role.DescriptionMaxLengthErrorMessage)
               .Matches(ValidationErrorMessages.Role.DescriptionAllowedCharactersPattern).WithMessage(ValidationErrorMessages.Role.DescriptionAllowedCharactersRegex)
               .Must(d => string.IsNullOrEmpty(d) || d == d.Trim()).WithMessage(ValidationErrorMessages.Role.DescriptionTrimErrorMessage);

            RuleFor(x => x.Permissions)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage(ValidationErrorMessages.Role.PermissionsListNullErrorMessage)
                .NotEmpty()
                .WithMessage(ValidationErrorMessages.Role.PermissionsListEmptyErrorMessage);
        }
    }
}
