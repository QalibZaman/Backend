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
            .NotEmpty().WithMessage("Invalid First Name. Please check format or length.")
            .NotNull().WithMessage("Invalid First Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid First Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches(@"^[^\s].*[^\s]$").WithMessage("Invalid First Name. Please check format or length.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Invalid Last Name. Please check format or length.")
                .NotNull().WithMessage("Invalid Last Name. Please check format or length.")
                .MinimumLength(2).WithMessage("Invalid Last Name. Please check format or length.")
                .MaximumLength(30).WithMessage("Invalid Last Name. Please check format or length.")
                .Matches("^[A-Za-z]+$").WithMessage("Invalid Last Name. Please check format or length.")
                .Matches("^[A-Z][a-z]*$").WithMessage("Invalid Last Name. Please check format or length.")
                .Matches(@"^[^\s].*[^\s]$").WithMessage("Invalid Last Name. Please check format or length.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Invalid Email. Please check format or length.")
                .NotNull().WithMessage("Invalid Email. Please check format or length.")
                .MaximumLength(256).WithMessage("Invalid Email. Please check format or length.")
                .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$")
                .WithMessage("Invalid Email. Please check format or length.");

            RuleFor(x => x.PhoneNumber)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
                .WithMessage("Invalid phone number. Please check format or length.");

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