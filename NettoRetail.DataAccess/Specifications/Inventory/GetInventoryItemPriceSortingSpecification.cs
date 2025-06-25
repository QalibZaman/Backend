using Microsoft.EntityFrameworkCore;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemPriceSortingSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemPriceSortingSpecification(int shopId, InventoryItemSortingEnum sortOption)
        : base(x => x.ShopId == shopId)
    {
        switch (sortOption)
        {
            case InventoryItemSortingEnum.PriceLowToHigh:
                AddOrderBy(x => x.Price, descending: false);
                break;
            case InventoryItemSortingEnum.PriceHighToLow:
                AddOrderBy(x => x.Price, descending: true);
                break;
            case InventoryItemSortingEnum.Newest:
            default:
                AddOrderBy(x => x.LastUpdatedDate, descending: true);
                break;
        }
    }
}