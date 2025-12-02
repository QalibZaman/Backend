using FluentValidation;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetPaginationRequestValidator : AbstractValidator<PaginationRequest>
    {
        public GetPaginationRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0.");
        }
    }
}
