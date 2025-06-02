namespace NettoRetail.Application.Services.INotificationService;

public interface INotificationService
{
    Task NotifyAllAsync(string message);
    Task NotifyGroupAsync(string shopId, string message);
    Task NotifyUserAsync(string userId, string message);
}
