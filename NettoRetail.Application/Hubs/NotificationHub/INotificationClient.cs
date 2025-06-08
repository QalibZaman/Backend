namespace NettoRetail.Application.Hubs.NotificationHub;

public interface INotificationClient
{
    Task ReceiveNotification(string message);
}
