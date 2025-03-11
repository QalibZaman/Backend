using FluentValidation;
using NettoRetail.Application.Models.Category;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetCategoriesRequestValidatior : AbstractValidator<GetCategoriesRequest>
{
    public GetCategoriesRequestValidatior()
    {
        RuleFor(model => model.PageNumber).GreaterThanOrEqualTo(0);
        RuleFor(model => model.PageSize).GreaterThanOrEqualTo(1);
    }
}
