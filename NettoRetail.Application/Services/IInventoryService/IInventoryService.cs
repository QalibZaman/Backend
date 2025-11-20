using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IInventoryService;

public interface IInventoryService
{
    bool IsItemNew(InventoryItemEntity item);
    bool IsItemOutOfStock(InventoryItemEntity item);
    bool IsItemOnSale(InventoryItemEntity item);
    Task<PaginatedResponse<InventoryItemListResponse>> GetShopItemsBySearchAsync(string query, int shopId,
        PaginationRequest pageModel, CancellationToken cancellationToken);
    Task<byte[]> DownloadInventoryItemsCsvAsync(int userShopId, CancellationToken cancellationToken = default);
}
