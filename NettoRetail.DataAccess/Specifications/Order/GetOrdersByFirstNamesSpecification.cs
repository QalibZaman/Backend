using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByFirstNamesSpecification : Specification<OrderEntity>
{
    public GetOrdersByFirstNamesSpecification(List<string> names)
        : base(x => names.Contains(x.Customer.FirstName))
    {
        AddInclude(x => x.Customer);
    }
}