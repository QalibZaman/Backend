using FluentValidation;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class EditPlatformAdminRequestValidator : AbstractValidator<EditPlatformAdminRequest>
{
  public EditPlatformAdminRequestValidator()
  {
        RuleFor(x => x.Permissions)
            .NotNull()
            .WithMessage("Permissions list cannot be null.")
            .NotEmpty()
            .WithMessage("At least one permission must be selected.");

        RuleForEach(x => x.Permissions)
            .NotNull()
            .WithMessage("Permission cannot be null.");

        RuleFor(x => x.Permissions)
            .Must(list => list.Select(p => p.GetEnumMemberValue()?.Trim().ToLowerInvariant())
                              .Distinct()
                              .Count() == list.Count)
            .WithMessage("Permissions must be unique.");
    }
}
