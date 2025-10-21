namespace NettoRetail.Application.Models.Request.RecentlyViewedProduct;
public class MergeRecentlyViewedItem
{
    public Guid InventoryItemId { get; set; }
    public DateTime ViewedAtUtc { get; set; }
}
