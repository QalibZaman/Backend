using System.Linq;
using FluentValidation;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class RoleSearchModelValidator : AbstractValidator<RoleSearchModel>
{
    private static readonly int[] AllowedPageSizes = { 10, 25, 50, 100 };

    public RoleSearchModelValidator()
    {
        Include(new PaginationRequestValidator());

        RuleFor(x => x.PageSize)
            .Must(size => AllowedPageSizes.Contains(size))
            .WithMessage($"Page size must be one of the following values: {string.Join(", ", AllowedPageSizes)}.");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MinimumLength(RoleValidationConstants.NameMinLength)
                .WithMessage($"Role name filter must be at least {RoleValidationConstants.NameMinLength} characters long.")
                .MaximumLength(RoleValidationConstants.NameMaxLength)
                .WithMessage($"Role name filter must be {RoleValidationConstants.NameMaxLength} characters or fewer.")
                .Matches(RoleValidationConstants.NameRegex)
                .WithMessage("Role name filter can only contain Latin letters, digits, '-' and '_' without spaces.");
        });
    }
}
