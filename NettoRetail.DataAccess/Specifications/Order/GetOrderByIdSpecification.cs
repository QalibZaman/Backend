using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrderByIdSpecification : Specification<OrderEntity>
{
    public GetOrderByIdSpecification(int id)
        : base(o => o.Id == id) { }
}
