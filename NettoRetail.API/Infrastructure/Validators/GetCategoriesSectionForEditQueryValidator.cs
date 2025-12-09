using FluentValidation;
using NettoRetail.Application.Models.Request.Category;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetCategoriesSectionForEditQueryValidator : AbstractValidator<GetCategoriesSectionForEditRequest>
    {
        public GetCategoriesSectionForEditQueryValidator()
        {
            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage("ShopId must be greater than 0.");
        }
    }
}
