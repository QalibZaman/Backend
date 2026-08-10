namespace NettoRetail.Domain.Entities;

public class BlacklistedTokenEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Token { get; set; }
    public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
}