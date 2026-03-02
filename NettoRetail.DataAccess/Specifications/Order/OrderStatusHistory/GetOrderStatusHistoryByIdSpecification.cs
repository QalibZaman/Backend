using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order.OrderStatusHistory;

public class GetOrderStatusHistoryByIdSpecification : Specification<OrderStatusHistoryEntity>
{
    public GetOrderStatusHistoryByIdSpecification(int orderId)
        : base(o => o.OrderId == orderId)
    {
        AddInclude(o => o.Include(o => o.UpdatedBy));
    }
}
