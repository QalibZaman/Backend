using NettoRetail.Application.Models.Request.RecentlyViewedProduct;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.Services.IRecentlyViewedService;
public interface IRecentlyViewedService
{
    Task AddOrUpdateAsync(Guid accountId, Guid inventoryItemId, CancellationToken cancellationToken);
    Task<List<InventoryItemCardResponse>> GetListAsync(Guid accountId, CancellationToken cancellationToken);
    Task MergeGuestItemsAsync(Guid accountId, IEnumerable<MergeRecentlyViewedItem> items, CancellationToken ct);
}
