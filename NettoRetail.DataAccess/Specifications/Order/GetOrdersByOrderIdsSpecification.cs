using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByOrderIdsSpecification : Specification<OrderEntity>
{
    public GetOrdersByOrderIdsSpecification(List<int> orderIds)
        : base(x => orderIds.Contains(x.Id)) { }
}