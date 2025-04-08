using FluentValidation;
using NettoRetail.Application.Models.Request.Role;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
               .Length(2,50).WithMessage("Role name length must be more than 2 and  less than 50 characters long.")
               .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Role name must only contain letters, digits, '-' and '_'.");

            RuleFor(x => x.Description)
               .MaximumLength(500).WithMessage("Description must be less than 500 characters long.")
               .Matches(@"^[A-Za-z0-9 ,.\-_'()]*$").WithMessage("Description must only contain letters, digits, spaces, and common punctuation.")
              .Matches(@"^[^\s].*[^\s]$").WithMessage("Description must not start or end with spaces.");
        }
    }
}