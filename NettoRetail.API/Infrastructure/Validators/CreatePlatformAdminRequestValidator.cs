using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreatePlatformAdminRequestValidator
    : AbstractValidator<CreatePlatformAdminRequest>
{
    private static readonly string[] AllowedPermissions = {
        Permissions.INVENTORY_MANAGEMENT,
        Permissions.CUSTOMER_MANAGEMENT,
        Permissions.ORDERS_MANAGEMENT,
        Permissions.ADMINS_MANAGEMENT,
        Permissions.SYSTEM_MANAGEMENT,
        Permissions.SUPERADMINS_MANAGEMENT

    };

    public CreatePlatformAdminRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("EM-32: First Name is required");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("EM-33: Last Name is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("EM-34: Email is required")
            .EmailAddress().WithMessage("EM-34: Invalid email format");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("EM-67: Invalid phone number");

        RuleFor(x => x.Permissions)
            .NotEmpty()
            .WithMessage("At least one permission must be selected");

        RuleForEach(x => x.Permissions)
            .Must(p => AllowedPermissions.Contains(p))
            .WithMessage("One or more invalid permissions selected");
    }
}
