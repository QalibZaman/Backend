using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByFirstNamesSpecification : Specification<OrderEntity>
{
    public GetOrdersByFirstNamesSpecification(List<string> names)
        : base(x => names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim().ToLower())
            .Any(name => x.Customer.FirstName.ToLower().Contains(name)))
    {
        AddInclude(x => x.Customer);
    }
}