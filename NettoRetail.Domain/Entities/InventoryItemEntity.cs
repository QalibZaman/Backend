using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class InventoryItemEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string ProductName { get; set; }
    public required long ProductUpc { get; set; }
    public required decimal Price { get; set; }
    public required string Description { get; set; }
    public required int CategoryId { get; set; }
    public required DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    public required bool IsActive { get; set; } = true;
    public required InventoryItemStatusEnum Status { get; set; } = InventoryItemStatusEnum.New;
    public required int AvailableAmount { get; set; }
    public required int ReservedAmount { get; set; }
    public required int OnHandAmount { get; set; }
    public required string ProductLocation { get; set; } = "No location";

    public required CategoryEntity Category { get; set; }
}