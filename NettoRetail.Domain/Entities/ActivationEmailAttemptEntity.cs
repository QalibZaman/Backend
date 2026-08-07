namespace NettoRetail.Domain.Entities;

public class ActivationEmailAttemptEntity
{
    public Guid Id { get; set; }

    public Guid StaffId { get; set; }
    public StaffEntity Staff { get; set; } = null!;

    public int SendCount { get; set; }
    public DateTime LastSentAt { get; set; }
}