namespace NettoRetail.Domain.Entities;

public class StaffEntity : BaseUserEntity
{
    public bool IsOnboardingCompleted { get; set; }
    public DateTime? InvitationTokenIssuedAt { get; set; }
}