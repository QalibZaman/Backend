using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.Services.IInventoryItemQueryService;
public interface IInventoryItemQueryService
{
    Task<PaginatedResponse<InventoryItemListResponse>> GetInventoryItemsAsync(
        int shopId,
        InventoryItemsSearchModel model,
        CancellationToken ct);
}
