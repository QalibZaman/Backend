using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByDateRangeSpecification : Specification<OrderEntity>
{
    public GetOrdersByDateRangeSpecification(DateTime from, DateTime to)
        : base(x => x.CreationDate >= from && x.CreationDate <= to) { }
}
