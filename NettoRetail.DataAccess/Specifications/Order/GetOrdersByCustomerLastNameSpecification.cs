using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByLastNamesSpecification : Specification<OrderEntity>
{
    public GetOrdersByLastNamesSpecification(List<string> lastNames)
        : base(x => lastNames
            .Where(lastName => !string.IsNullOrWhiteSpace(lastName))
            .Select(lastName => lastName.Trim().ToLower())
            .Any(lastName => x.Customer.LastName.ToLower().Contains(lastName)))
    {
        AddInclude(x => x.Customer);
    }
}