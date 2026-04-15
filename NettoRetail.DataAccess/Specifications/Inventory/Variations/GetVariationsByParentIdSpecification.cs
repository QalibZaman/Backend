using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory.Variations;

public class GetVariationsByParentIdSpecification : Specification<InventoryItemEntity>
{
    public GetVariationsByParentIdSpecification(Guid parentId, int shopId)
        : base(v => v.IsVariation && v.ParentItemId == parentId && v.ShopId == shopId) { }
}
