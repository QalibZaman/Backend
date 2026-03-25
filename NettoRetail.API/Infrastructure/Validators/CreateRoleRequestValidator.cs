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
               .NotEmpty().WithMessage("Role name must be provided.")
               .Length(RoleValidationConstants.NameMinLength, RoleValidationConstants.NameMaxLength)
               .WithMessage($"Role name must be between {RoleValidationConstants.NameMinLength} and {RoleValidationConstants.NameMaxLength} characters long.")
               .Matches(RoleValidationConstants.NameRegex)
               .WithMessage("Role name must only contain Latin letters, digits, '-' and '_' without spaces.");

            RuleFor(x => x.Description)
               .MaximumLength(RoleValidationConstants.DescriptionMaxLength)
               .WithMessage($"Description must be less than {RoleValidationConstants.DescriptionMaxLength} characters long.")
               .Matches(RoleValidationConstants.DescriptionRegex)
               .WithMessage("Description must only contain Latin letters, digits, spaces, and common punctuation.")
               .Must(d => string.IsNullOrEmpty(d) || d == d.Trim()).WithMessage("Description must not start or end with spaces.");

            RuleFor(x => x.PermissionIds)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("At least one permission must be provided.")
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("PermissionIds must contain unique values.");
        }
    }
}
