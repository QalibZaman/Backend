using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory.Variations;

public class GetVariationsByParentIdSpecification : Specification<InventoryItemEntity>
{
    public GetVariationsByParentIdSpecification(Guid parentId)
        : base(v => v.IsVariation == true && v.ParentItemId == parentId) { }
}
