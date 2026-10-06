namespace NettoRetail.Application.Models.Request.Notification;

public class UpdateNotificationPreferenceRequest
{
    public required int NotificationTypeId { get; init; }
    public required bool IsEnabled { get; init; }
}