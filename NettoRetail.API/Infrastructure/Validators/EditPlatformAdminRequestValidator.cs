using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class EditPlatformAdminRequestValidator : AbstractValidator<EditPlatformAdminRequest>
{
  public EditPlatformAdminRequestValidator()
  {
    RuleFor(x => x.Permissions)
        .NotNull()
        .WithMessage("Permissions object cannot be null.")
        .NotEmpty()
        .WithMessage("At least one permission must be selected.");
  }
}
