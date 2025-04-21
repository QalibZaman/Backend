using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByCustomerIdsSpecification : Specification<OrderEntity>
{
    public GetOrdersByCustomerIdsSpecification(List<Guid> customerIds)
        : base(x => customerIds.Contains(x.CustomerId)) { }
}