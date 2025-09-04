using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class AccountEntity
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
    public bool TwoFactorEnabled { get; set; }
    public string? EncryptedTotpSecret { get; set; }
    public DateTime? PasswordUpdatedAt { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string? StripeCustomerId { get; set; }
    public bool IsSubscribedToNewsletter { get; set; }
    public int? SubscriptionPlanId { get; set; }
    public bool IsOnboardingCompleted { get; set; } = false;

    public SubscriptionPlanEntity SubscriptionPlan { get; set; }
    public CountryEntity? Country { get; set; }
    public RoleEntity Role { get; set; } = null!;
    public int? ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
    public FailedAuthAttemptEntity FailedAuthAttempt { get; set; } = null!;
    public ICollection<SessionEntity>? Sessions { get; set; }
    public ICollection<PaymentMethodEntity>? PaymentMethods { get; set; }
    public ICollection<PasswordHistoryEntity> PasswordHistories { get; set; } = [];
    public ICollection<ReviewEntity> Reviews { get; set; } = [];
    public ICollection<FeedbackEntity>? Feedbacks { get; set; }

    public string FullName => FirstName + " " + LastName;
}