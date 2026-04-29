using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class BaseUserEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ShippingAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public int? CountryId { get; set; }
    public int RoleId { get; set; }
    public string? ProfileImageUrl { get; set; }
    public StaffStatusEnum Status { get; set; } = StaffStatusEnum.CreatePassword;
    public DateTime? PasswordUpdatedAt { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string? StripeCustomerId { get; set; }
    public bool IsSubscribedToNewsletter { get; set; }

    public CountryEntity? Country { get; set; }
    public RoleEntity Role { get; set; } = null!;
    public int? ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
    public FailedAuthAttemptEntity FailedAuthAttempt { get; set; } = null!;
    public ICollection<SessionEntity>? Sessions { get; set; }
    public ICollection<PaymentMethodEntity>? PaymentMethods { get; set; }
    public ICollection<PasswordHistoryEntity> PasswordHistories { get; set; } = [];
    public ICollection<FeedbackEntity>? Feedbacks { get; set; }
    public ICollection<FavouriteListItemEntity> FavouriteListItems { get; set; } = [];
    public CommunicationPreferenceEntity? CommunicationPreference { get; set; }
    public ICollection<RecentlyViewedProductEntity> RecentlyViewedProducts { get; set; } = [];
    public ICollection<AddressEntity> Addresses { get; set; }
    public ItemComparisonEntity? ComparedItems { get; set; }
    public string FullName => FirstName + " " + LastName;

    public bool IsOnline()
    {
        if (Status == StaffStatusEnum.Online) return true;
        return false;

    }
}