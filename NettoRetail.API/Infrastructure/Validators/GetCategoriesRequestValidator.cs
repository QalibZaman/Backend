using FluentValidation;
using NettoRetail.Application.Models.Request.Category;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetCategoriesRequestValidator : AbstractValidator<GetCategoriesRequest>
{
    public GetCategoriesRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .Must(size => CategoryConstants.AllowedPageSizes.Contains(size))
            .WithMessage($"Page size must be one of the following values: {string.Join(", ", CategoryConstants.AllowedPageSizes)}.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(50)
            .WithMessage("Search term cannot exceed 50 characters.");
    }
}