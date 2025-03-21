using FluentValidation;
using NettoRetail.Application.Models.Request.Role;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
    {
        public CreateRoleRequestValidator()
        {
            RuleFor(x => x.Name)
               .MaximumLength(50).WithMessage("Product name length must be less than 50 characters long.")
               .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Product name must only contain letters, digits, '-' and '_'.");
        }
    }
}
