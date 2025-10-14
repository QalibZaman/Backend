using Microsoft.Extensions.Options;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services.IInventoryService;

public class InventoryService : IInventoryService
{
    private readonly InventoryItemOptions _options;

    public InventoryService(IOptions<InventoryItemOptions> options)
    {
        _options = options.Value;
    }

    public bool IsItemNew(InventoryItemEntity item) => item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-_options.NewItemDaysThreshold);
    public bool IsItemOutOfStock(InventoryItemEntity item) => item.AvailableAmount <= 0;
    public bool IsItemOnSale(InventoryItemEntity item) => item.PromotionPrice.HasValue;
}
