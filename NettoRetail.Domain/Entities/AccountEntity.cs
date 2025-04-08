using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class AccountEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public int RoleId { get; set; }
    public StaffStatusEnum Status { get; set; } = StaffStatusEnum.CreatePassword;
    public bool TwoFactorEnabled { get; set; }
    public string? EncryptedTotpSecret { get; set; }
    public DateTime? PasswordUpdatedAt { get; set; }

    public RoleEntity Role { get; set; } = null!;
    public int? ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
    public FailedAuthAttemptEntity FailedAuthAttempt { get; set; } = null!;
    public ICollection<SessionEntity>? Sessions { get; set; }
}