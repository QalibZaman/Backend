using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByUpcsSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByUpcsSpecification(IEnumerable<string> upcs, int? shopId = null)
        : base(x => upcs.Contains(x.ProductUpc) && (!shopId.HasValue || x.ShopId == shopId.Value))
    {
    }
}
