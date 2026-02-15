namespace NettoRetail.Domain.Entities;

public class ShopEntity
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Url { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Description { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public int? CountryId { get; set; }
    public required bool IsActive { get; set; }
    public string? DeactivationReason { get; set; }
    public DateTime? DeactivationDate { get; set; }
    public required int SubscriptionPlanId { get; set; }

    public bool CategoriesSectionVisibility { get; set; }
    public string? CategoriesSectionName { get; set; }

    public SubscriptionPlanEntity? SubscriptionPlan { get; set; }
    public ICollection<RoleEntity>? Roles { get; set; }
    public ICollection<CategoryEntity>? Categories { get; set; }
    public CountryEntity? Country { get; set; }
    public ICollection<AccountEntity>? Account { get; set; }
    public ICollection<InventoryItemEntity>? InventoryItems { get; set; }
    public ICollection<BrandEntity>? Brands { get; set; }
    public void Activate()
    {
        IsActive = true;
        DeactivationReason = null;
        DeactivationDate = null;
    }

    public void Deactivate(string reason, DateTime deactivationDate)
    {
        IsActive = false;
        DeactivationReason = reason;
        DeactivationDate = deactivationDate;
    }

}