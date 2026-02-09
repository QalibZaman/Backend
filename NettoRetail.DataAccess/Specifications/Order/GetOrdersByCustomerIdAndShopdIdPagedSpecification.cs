using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByCustomerIdAndShopdIdPagedSpecification : Specification<OrderEntity>
{
    public GetOrdersByCustomerIdAndShopdIdPagedSpecification(
        Guid customerId,
        int shopId,
        int skip,
        int take)
        : base(o => o.CustomerId == customerId && o.ShopId == shopId)
    {
        AddOrderByDescending(o => o.CreationDate);
        Skip = skip;
        Take = take;
    }
}