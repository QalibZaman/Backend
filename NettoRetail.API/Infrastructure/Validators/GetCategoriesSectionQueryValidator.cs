using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetCategoriesSectionQueryValidator : AbstractValidator<GetCategoriesSectionRequest>
    {
        public GetCategoriesSectionQueryValidator()
        {
            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage("ShopId must be greater than 0.");
        }
    }
}
