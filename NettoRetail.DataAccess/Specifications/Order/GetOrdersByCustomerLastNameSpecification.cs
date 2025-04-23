using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByLastNamesSpecification : Specification<OrderEntity>
{
    public GetOrdersByLastNamesSpecification(List<string> lastNames)
        : base(x => lastNames.Contains(x.Customer.Account.LastName))
    {
        AddInclude(x => x.Customer.Account);
    }
}