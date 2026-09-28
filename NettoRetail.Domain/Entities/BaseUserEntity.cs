using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public abstract class BaseUserEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public int RoleId { get; set; }
    public RoleEntity Role { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public FailedAuthAttemptEntity FailedAuthAttempt { get; set; } = null!;
    public StaffStatusEnum Status { get; set; } = StaffStatusEnum.CreatePassword;
    public DateTime? PasswordUpdatedAt { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string? StripeCustomerId { get; set; }
    public bool IsSubscribedToNewsletter { get; set; }
    public int? ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
    public ICollection<PasswordHistoryEntity> PasswordHistories { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}";
    public bool IsOnline()
    {
        return Status == StaffStatusEnum.Online;
    }
}