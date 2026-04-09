namespace NettoRetail.Domain.Entities;

public class StaffEntity : BaseUserEntity
{
    public bool TwoFactorEnabled { get; set; }
    public string? EncryptedTotpSecret { get; set; }
    public bool IsOnboardingCompleted { get; set; } = false;
    public DateTime? InvitationTokenIssuedAt { get; set; }
    public ICollection<ReviewEntity> Reviews { get; set; } = [];
    public ICollection<EntityLock> Locks { get; set; } = [];
}
