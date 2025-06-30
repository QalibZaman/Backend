namespace NettoRetail.Application.Services.INotificationService;

public interface INotificationService
{
    Task NotifyAllAsync(string message);
    Task NotifyShopGroupAsync(string shopId, string message);
    Task NotifyUserAsync(string userId, string message);
}
