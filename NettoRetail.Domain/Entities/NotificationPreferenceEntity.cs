namespace NettoRetail.Domain.Entities;

public class NotificationPreferenceEntity
{
    public Guid Id { get; set; }

    public Guid StaffId { get; set; }
    public StaffEntity Staff { get; set; } = null!;

    public int NotificationTypeId { get; set; }
    public NotificationTypeEntity NotificationType { get; set; } = null!;

    public bool IsEnabled { get; set; }
}