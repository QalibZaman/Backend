using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByCustomerIdAndShopdIdPagedSpecification:Specification<OrderEntity>
{
    public GetOrdersByCustomerIdAndShopdIdPagedSpecification(Guid customerId, int shopId) 
        : base(x=> x.CustomerId == customerId && x.ShopId == shopId)
    {
        AddOrderByDescending(o => o.CreationDate);
    }
}