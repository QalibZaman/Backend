using NettoRetail.Domain.Entities.JunctionEntities;
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
    public decimal? PromotionPrice { get; set; }
    public bool IsPromoted { get; set; }
    public required DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    public required bool IsActive { get; set; } = true;
    public required InventoryItemStatusEnum Status { get; set; } = InventoryItemStatusEnum.Available;
    public required int AvailableAmount { get; set; }
    public required int ReservedAmount { get; set; }
    public required int OnHandAmount { get; set; }
    public required string ProductLocation { get; set; } = "No location";
    public required int ShopId { get; set; }

    public required CategoryEntity Category { get; set; }
    public ShopEntity? Shop { get; set; }
    public ICollection<OrderItemEntity> OrderItems { get; set; } = null!;
    public required int? BrandId { get; set; }
    public required BrandEntity? Brand { get; set; }
    public ICollection<InventoryItemImageEntity>? Images { get; set; }
}