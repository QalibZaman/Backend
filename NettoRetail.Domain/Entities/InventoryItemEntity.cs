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
    public bool HasVariations { get; set; }
    public bool IsVariation { get; set; }
    public bool Exists { get; set; } = true;

    public Guid? ParentItemId { get; set; } = null;
    public required CategoryEntity Category { get; set; }
    public ShopEntity? Shop { get; set; }
    public ICollection<OrderItemEntity> OrderItems { get; set; } = null!;
    public required int? BrandId { get; set; }
    public required BrandEntity? Brand { get; set; }
    public ICollection<InventoryItemImageEntity> Images { get; set; } = [];
    public ICollection<FavouriteListItemEntity>? FavouriteListItems { get; set; }
    public ICollection<RecentlyViewedProductEntity> RecentlyViewedByUsers { get; set; } = [];
    public ICollection<ProductDetailEntity> ProductDetails { get; set; } = [];
    public ICollection<ItemComparisonItemEntity> ComparisonItems { get; set; } = null!;
    public InventoryItemEntity? ParentItem { get; set; }
    public ICollection<InventoryItemEntity>? ChildItems { get; set; } = [];

    public void UpdateStatus()
    {
        Status = IsActive ? InventoryItemStatusEnum.Available : InventoryItemStatusEnum.Unavailable;
        LastUpdatedDate = DateTime.UtcNow;
    }
    public void ApplyUpdate(InventoryItemUpdateData data)
    {
        if (string.IsNullOrWhiteSpace(data.ProductName))
            throw new InvalidOperationException("Product name cannot be empty.");

        if (string.IsNullOrWhiteSpace(data.Description))
            throw new InvalidOperationException("Description cannot be empty.");

        ProductUpc = data.ProductUpc;
        ProductName = data.ProductName.Trim();
        CategoryId = data.CategoryId;
        BrandId = data.BrandId;
        Description = data.Description.Trim();

        Price = data.Price;
        IsActive = data.IsActive;

        IsPromoted = data.IsPromoted;
        PromotionPrice = data.PromotionPrice;

        UpdateStatus();
    }




}