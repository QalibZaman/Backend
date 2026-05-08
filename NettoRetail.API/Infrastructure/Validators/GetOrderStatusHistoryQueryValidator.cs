using FluentValidation;
using NettoRetail.Application.CQRS.Orders.Queries;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetOrderStatusHistoryQueryValidator : AbstractValidator<GetOrderStatusHistoryQuery>
    {
        public GetOrderStatusHistoryQueryValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0);
            RuleFor(x => x.ShopId)
                .GreaterThan(0);
        }
    }
}
