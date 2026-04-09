using FluentValidation;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Role name must be provided.")
                .Length(2, 50)
                .WithMessage("Role name must be between 2 and 50 characters long.")
                .Matches("^[A-Za-z0-9_-]+$")
                .WithMessage("Role name can only contain Latin letters, digits, '-' and '_' without spaces.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description must not exceed 500 characters.")
                .Matches(@"^[A-Za-z0-9 ,.\-_'();]*$")
                .WithMessage("Description can only contain Latin letters, digits, spaces, and common punctuation.")
                .Must(d => !(d.StartsWith(' ') || d.EndsWith(' ')))
                .WithMessage("Description must not start or end with spaces.");

            RuleFor(x => x.PermissionIds)
                .NotEmpty()
                .WithMessage("At least one permission ID must be provided.")
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("Duplicate IDs are not allowed.");
        }
    }
}
