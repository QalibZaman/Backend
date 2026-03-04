using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Domain.Entities;

public class ItemComparisonEntity
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public ICollection<ItemComparisonItemEntity> ComparisonItems { get; set; } = null!;
    public BaseUserEntity? User { get; set; }

    public void AddItem(Guid itemId)
    {
        ComparisonItems.Add(new ItemComparisonItemEntity
        {
            InventoryItemId = itemId,
            ComparisonId = Id
        });
    }

    public ItemComparisonItemEntity? GetItem(Guid itemId)
    {
        var item = ComparisonItems.FirstOrDefault(item => item.InventoryItemId == itemId);

        return item;
    }
}