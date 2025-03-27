using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsRelatedCategories : Specification<InventoryItemEntity>
{
    public GetInventoryItemsRelatedCategories(int userShopId)
        : base(x => x.ShopId == userShopId)
    {
        AddInclude(x => x.Category);
    }
}