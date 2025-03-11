namespace NettoRetail.Domain.Entities;

public class InventoryItemEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string ProductName { get; set; }
    public required long ProductUpc { get; set; }
    public required decimal Price { get; set; }
    public required string Description { get; set; }
    public required int CategoryId { get; set; }
    public DateTime? LastUpdatedDate { get; set; }
    public required bool IsActive { get; set; }

    public required CategoryEntity Category { get; set; }
}