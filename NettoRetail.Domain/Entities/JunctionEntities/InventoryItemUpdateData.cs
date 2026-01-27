namespace NettoRetail.Domain.Entities.JunctionEntities;

public record InventoryItemUpdateData(
    long ProductUpc,
    string ProductName,
    int CategoryId,
    int BrandId,
    string Description,
    bool IsActive,
    decimal Price,
    bool IsPromoted,
    decimal? PromotionPrice
);