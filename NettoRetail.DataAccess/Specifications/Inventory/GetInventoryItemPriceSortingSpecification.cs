using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemPriceSortingSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemPriceSortingSpecification(int shopId)
        : base(x => x.ShopId == shopId)
    {
        AddOrderBy(x => x.Price, false);
    }
}