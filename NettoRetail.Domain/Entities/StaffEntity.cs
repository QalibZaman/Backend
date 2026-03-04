namespace NettoRetail.Domain.Entities;

public class StaffEntity : BaseUserEntity
{
    public bool TwoFactorEnabled { get; set; }
    public string? EncryptedTotpSecret { get; set; }
    public bool IsOnboardingCompleted { get; set; } = false;
    public ICollection<ReviewEntity> Reviews { get; set; } = [];
    public ICollection<InvitationTokenEntity> InvitationTokens { get; set; } = [];
    public ICollection<EntityLock> Locks { get; set; } = [];
}