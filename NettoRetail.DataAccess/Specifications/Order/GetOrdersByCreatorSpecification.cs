using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Order;

public class GetOrdersByCreatorSpecification : Specification<OrderEntity>
{
    public GetOrdersByCreatorSpecification(List<string> creators)
        : base(x => creators
            .Where(creator => !string.IsNullOrWhiteSpace(creator))
            .Select(creator => creator.Trim().ToLower())
            .Any(creator => x.Creator.ToLower().Contains(creator))) { }
}