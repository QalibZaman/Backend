using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetItemsByIdsSpecification : Specification<InventoryItemEntity>
{
    public GetItemsByIdsSpecification(IEnumerable<Guid> ids)
        : base(x => ids.Contains(x.Id)) { }
}