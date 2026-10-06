namespace NettoRetail.Domain.Entities;

public class NotificationTypeEntity
{
    public int Id { get; set; }

    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<NotificationEntity> Notifications { get; set; } = [];
    public ICollection<NotificationPreferenceEntity> Preferences { get; set; } = [];
}