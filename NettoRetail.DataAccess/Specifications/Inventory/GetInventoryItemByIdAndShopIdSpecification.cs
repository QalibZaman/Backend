using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemByIdAndShopIdSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemByIdAndShopIdSpecification(Guid id, int shopId)
        : base(x => x.Id == id && x.ShopId == shopId) { }
}
