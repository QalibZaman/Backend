using FluentValidation;
using NettoRetail.Application.Models.Request.Offer;

namespace NettoRetail.API.Infrastructure.Validators;

public class SearchSuggestionsRequestValidator : AbstractValidator<SearchSuggestionsRequest>
{
    public SearchSuggestionsRequestValidator()
    {
        RuleFor(x => x.ShopId)
         .NotEmpty()
         .Must(x => x > 0)
         .WithMessage("ShopId must be greater than 0.");
    }
}
