using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class GetSortedInventoryItemsModel
{
    public InventoryItemSortingEnum SortOption { get; set; }
    public int ShopId { get; set; }
}