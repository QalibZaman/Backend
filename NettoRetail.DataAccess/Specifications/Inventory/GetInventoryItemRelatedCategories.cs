using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemRelatedCategories : Specification<InventoryItemEntity>
{
    public GetInventoryItemRelatedCategories()
        : base(Empty)
    {
        AddInclude(x => x.Category);
    }
}