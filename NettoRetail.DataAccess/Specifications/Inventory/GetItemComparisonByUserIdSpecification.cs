using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetItemComparisonByUserIdSpecification : Specification<ItemComparisonEntity>
{
    public GetItemComparisonByUserIdSpecification(Guid userId)
        : base(c => c.UserId == userId) { }
}
