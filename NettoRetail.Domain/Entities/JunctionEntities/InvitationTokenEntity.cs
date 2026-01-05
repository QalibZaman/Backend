namespace NettoRetail.Domain.Entities.JunctionEntities;

public class InvitationTokenEntity
{
    public Guid Id { get; set; }

    public Guid AccountId;

    public AccountEntity AccountEntity;

    public required string TokenHash;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? InvalidatedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}