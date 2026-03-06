using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByIdSpecification : Specification<OrderEntity>
{
    public GetOrdersByIdSpecification(int id)
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Customer);
    }
}
