using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ProductReviewEntity
{
    public Guid Id { get; set; }

    public required string ProductUpc { get; set; }

    public required int Rating { get; set; }

    public required string Title { get; set; }

    public string? ReviewText { get; set; }

    public string? VariantInfo { get; set; }

    public string? PhotoUrls { get; set; }
    public required DateTime DateSubmitted { get; set; }
    public required ReviewStatusEnum ReviewStatus { get; set; }
    public required Guid CustomerId { get; set; }
    public BaseUserEntity Customer { get; set; } = null!;
    public Guid InventoryItemId { get; set; }
    public InventoryItemEntity InventoryItem { get; set; } = null!;
}