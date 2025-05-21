namespace NettoRetail.Domain.Entities;
public class PasswordHistoryEntity
{
    public int Id { get; set; }
    public Guid AccountId { get; set; }
    public required string HashedPassword { get; set; }
    public DateTime CreatedAt { get; set; }

    public AccountEntity Account { get; set; } = null!;
}
