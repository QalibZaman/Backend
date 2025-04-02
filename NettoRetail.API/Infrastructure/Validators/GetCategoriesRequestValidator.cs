using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetCategoriesRequestValidator : AbstractValidator<GetCategoriesRequest>
{
    public GetCategoriesRequestValidator()
    {
        RuleFor(model => model.PageNumber).GreaterThanOrEqualTo(0);
        RuleFor(model => model.PageSize).GreaterThanOrEqualTo(1);
    }
}
