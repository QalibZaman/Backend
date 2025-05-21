using FluentValidation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Models.Request.Staff;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetFilteredStaffValidator : AbstractValidator<GetFilteredStaffRequest>
{
    public GetFilteredStaffValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(30).WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches(@"^\S.*\S$|^\S$").WithMessage("Invalid First Name. Please check format or length.");

        RuleFor(x => x.LastName)
            .MaximumLength(30).WithMessage("Invalid Last Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid Last Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid Last Name. Please check format or length.")
            .Matches(@"^\S.*\S$|^\S$").WithMessage("Invalid Last Name. Please check format or length.");

        RuleFor(x => x.Email)
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters long.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$").WithMessage("Email must be in a valid format.");

        RuleFor(x => x.Status)
            .MustBeAValidAccountStatus().WithMessage("Invalid status value provided.");

        RuleForEach(x => x.SortingOptions).ChildRules(sorting =>
        {
            sorting.RuleFor(x => x.Field)
                .NotEmpty().WithMessage("Sorting field is required.");
        });
    }
}
