using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByShopIdSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByShopIdSpecification(int shopId)
    : base(x => x.ShopId == shopId) { }
}
