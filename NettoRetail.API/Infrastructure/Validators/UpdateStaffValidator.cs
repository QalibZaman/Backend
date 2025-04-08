using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateStaffValidator : AbstractValidator<UpdateStaffRequest>
{
    public UpdateStaffValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(30).WithMessage("First name must be no more than 30 characters.")
            .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$").WithMessage("First name must start with a capital letter and only contain Latin letters. Spaces are allowed between words.")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(30).WithMessage("Last name must be no more than 30 characters.")
            .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$").WithMessage("Last name must start with a capital letter and only contain Latin letters. Spaces are allowed between words.")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Email)
            .MaximumLength(256).WithMessage("Email must be no more than 256 characters.")
            .Matches(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$")
            .WithMessage("Invalid email format. Must be a valid email like name@domain.com.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("The role id must be greater than 0")
            .When(x => x.RoleId.HasValue);

        RuleFor(x => x.IsActive)
            .NotNull()
            .When(x => x.IsActive.HasValue)
            .WithMessage("Activation status must be specified.");
    }
}
