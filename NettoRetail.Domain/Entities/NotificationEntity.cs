namespace NettoRetail.Domain.Entities;

public class NotificationEntity
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public StaffEntity Staff { get; set; } = null!;
    public int NotificationTypeId { get; set; }
    public NotificationTypeEntity NotificationType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; }
    public string? Url { get; set; }
}