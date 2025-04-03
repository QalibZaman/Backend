using FluentValidation;
using NettoRetail.Application.Models.Request.Customer;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetCustomersRequestValiddator : AbstractValidator<GetCustomersRequest>
{
    public GetCustomersRequestValiddator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Page.HasValue)
            .WithMessage("Page can not be less than 0");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageSize.HasValue)
            .WithMessage("Page size can not be less than 1");
    }
}
