using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IInventoryService;

public interface IInventoryService
{
    bool IsItemNew(InventoryItemEntity item);
    bool IsItemOutOfStock(InventoryItemEntity item);
    bool IsItemOnSale(InventoryItemEntity item);
}
