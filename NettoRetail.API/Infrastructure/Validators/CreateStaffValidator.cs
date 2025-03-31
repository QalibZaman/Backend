using FluentValidation;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateStaffValidator : AbstractValidator<CreateStaffRequest>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.FirstName)
             .NotEmpty().WithMessage("First name is required.")
             .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
             .MaximumLength(30).WithMessage("First name must be no more than 30 characters.")
             .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$")
             .WithMessage("First name must start with a capital letter and only contain Latin letters. Spaces are allowed between words.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(30).WithMessage("Last name must be no more than 30 characters.")
            .Matches(@"^([A-Z][a-z]*)( [A-Z][a-z]*)*$")
            .WithMessage("Last name must start with a capital letter and only contain Latin letters. Spaces are allowed between words.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must be no more than 256 characters.")
            .Matches(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$")
            .WithMessage("Invalid email format. Must be a valid email like name@domain.com.");

        RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("The role id must be greater than 0");
    }
}
