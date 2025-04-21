using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByCreatorSpecification : Specification<OrderEntity>
{
    public GetOrdersByCreatorSpecification(List<string> creators)
        : base(x => creators.Contains(x.Creator)) { }
}