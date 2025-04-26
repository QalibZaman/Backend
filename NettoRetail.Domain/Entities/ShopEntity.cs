using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class ShopEntity
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Url { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string ZipCode { get; set; }
    public required string Description { get; set; }
    public int CountryId { get; set; }
    public SubscriptionTypeEnum Subscription { get; set; } = SubscriptionTypeEnum.Free;

    public ICollection<RoleEntity>? Roles { get; set; }
    public ICollection<CategoryEntity>? Categories { get; set; }
    public required CountryEntity Country { get; set; }
    public ICollection<AccountEntity>? Account { get; set; }
    public ICollection<InventoryItemEntity>? InventoryItems { get; set; }
    public ICollection<BrandEntity>? Brands { get; set; }
}
