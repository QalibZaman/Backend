using System.ComponentModel.DataAnnotations.Schema;

namespace NettoRetail.Domain.Entities;

public class FailedAuthAttemptEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("StaffId")]
    public Guid AccountId { get; set; }
    public int FailedAttempts { get; set; } = 0;
    public DateTime? LockoutEndTime { get; set; }
    public DateTime LastFailedTime { get; set; } = DateTime.UtcNow;
    public BaseUserEntity Account { get; set; } = null!;
}