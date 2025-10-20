namespace NettoRetail.Application.Models.Request.RecentlyViewedProduct;
public class MergeRecentlyViewedRequest
{
    public List<MergeRecentlyViewedItem> Items { get; set; } = new();
}
