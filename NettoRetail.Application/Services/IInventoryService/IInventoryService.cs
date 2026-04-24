using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IInventoryService;

public interface IInventoryService
{
    bool IsItemNew(InventoryItemEntity item);
    bool IsItemOutOfStock(InventoryItemEntity item);
    bool IsItemOnSale(InventoryItemEntity item);
    Task<PaginatedResponse<InventoryItemListResponse>> GetShopItemsBySearchAsync(string query, int shopId, PaginationRequest pageModel, CancellationToken cancellationToken);
    Task<byte[]> DownloadInventoryItemsCsvAsync(int userShopId, CancellationToken cancellationToken = default);

    Task ValidateUpdateAsync(UpdateInventoryItemModel model, int shopId, Guid id, InventoryItemEntity inventoryItem, CancellationToken cancellationToken);
    List<InventoryItemEntity> BuildVariationInventoryItemEntities(ICollection<VariationModel> models, InventoryItemEntity parentItem);
    Task<bool> AreAllItemsBelongToShopAsync(int shopId, IEnumerable<Guid> inventoryItemsIds, CancellationToken cancellationToken);
    Task<ToggleLinkProductsResponse> ToggleLinkProductsAsync(Guid parentId, Guid childId, int shopId, Dictionary<string, string>? conflictResolutions, CancellationToken cancellationToken);
}
