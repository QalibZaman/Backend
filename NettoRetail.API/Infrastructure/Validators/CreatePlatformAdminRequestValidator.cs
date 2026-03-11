using FluentValidation;
using NettoRetail.Application.Extensions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Permissions;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class CreatePlatformAdminRequestValidator
        : AbstractValidator<CreatePlatformAdminRequest>
    {
        public CreatePlatformAdminRequestValidator(
            IStaffRepository accountRepo,
            IPermissionRepository permissionRepo)
        {
            RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("EM-32")
            .NotNull().WithMessage("EM-32")
            .MinimumLength(2).WithMessage("EM-32")
            .MaximumLength(30).WithMessage("EM-32")
            .Matches("^[A-Za-z]+$").WithMessage("EM-32")
            .Matches("^[A-Z][a-z]*$").WithMessage("EM-32")
            .Matches(@"^[^\s].*[^\s]$").WithMessage("EM-32");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("EM-33")
                .NotNull().WithMessage("EM-33")
                .MinimumLength(2).WithMessage("EM-33")
                .MaximumLength(30).WithMessage("EM-33")
                .Matches("^[A-Za-z]+$").WithMessage("EM-33")
                .Matches("^[A-Z][a-z]*$").WithMessage("EM-33")
                .Matches(@"^[^\s].*[^\s]$").WithMessage("EM-33");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("EM-34")
                .NotNull().WithMessage("EM-34")
                .MaximumLength(256).WithMessage("EM-34")
                .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$").WithMessage("EM-34");

            RuleFor(x => x.PhoneNumber)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("EM-67");

            RuleFor(x => x.Permissions)
                 .NotNull()
                    .WithMessage("Permissions must be provided.")
                    .NotEmpty().WithMessage("At least one permission must be selected");

            RuleForEach(x => x.Permissions)
                .IsInEnum()
                .WithMessage("Invalid permission value.");

            RuleFor(x => x)
                .Custom((model, context) =>
                {
                    if (string.IsNullOrWhiteSpace(model.FirstName)
                        && string.IsNullOrWhiteSpace(model.LastName)
                        && string.IsNullOrWhiteSpace(model.Email))
                    {
                        context.AddFailure("Please, fill in all required fields");
                    }
                });
        }
    }
}
