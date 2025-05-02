using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByCustomerIdSpecification : Specification<OrderEntity>
{
    public GetOrdersByCustomerIdSpecification(Guid customerId)
    : base(x => x.CustomerId == customerId)
    { }
}
