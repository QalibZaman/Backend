using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class EditPlatformAdminRequestValidator : AbstractValidator<EditPlatformAdminRequest>
{
    public EditPlatformAdminRequestValidator()
    {
        RuleFor(x => x.AssignedPermissions)
            .NotNull()
            .WithMessage("Permissions list cannot be null.")
            .Must(p => p.Any())
            .WithMessage("At least one permission must be selected");
    }
}
