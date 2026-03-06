namespace NettoRetail.Domain.Entities;

public class InvitationTokenEntity
{
    public Guid Id { get; set; }

    public Guid AccountId;

    public StaffEntity StaffEntity;

    public required string TokenHash;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? InvalidatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}