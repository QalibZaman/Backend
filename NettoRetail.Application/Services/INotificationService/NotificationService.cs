using Microsoft.AspNetCore.SignalR;
using NettoRetail.Application.Hubs.NotificationHub;

namespace NettoRetail.Application.Services.INotificationService;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

    public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAllAsync(string message)
    {
        await _hubContext.Clients.All.ReceiveNotification(message);
    }

    public async Task NotifyShopGroupAsync(string shopId, string message)
    {
        var group = $"shop:{shopId}";
        await _hubContext.Clients.Group(group).ReceiveNotification(message);
    }

    public async Task NotifyUserAsync(string userId, string message)
    {
        await _hubContext.Clients.User(userId).ReceiveNotification(message);
    }
}
