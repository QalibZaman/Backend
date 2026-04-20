using FluentValidation;
using NettoRetail.Application.CQRS.Orders.Queries;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class GetOrderProductsQueryValidator : AbstractValidator<GetOrderProductsQuery>
    {
        public GetOrderProductsQueryValidator()
        {
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0);

            RuleFor(x => x.UserShopId)
                .GreaterThan(0);
        }
    }
}
