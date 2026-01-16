using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory.Variations;

public class GetVariationsByParentIdAndExistenceSpecification : Specification<InventoryItemEntity>
{
    public GetVariationsByParentIdAndExistenceSpecification(Guid parentId, bool exists)
        : base(v => v.Exists == exists && v.IsVariation == true && v.ParentItemId == parentId) { }
}
