using Microsoft.AspNetCore.SignalR;
using NettoRetail.Application.Hubs.NotificationHub;

namespace NettoRetail.Application.Services.INotificationService;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly WebSocketConnectionManager _connectionManager;

    public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext, WebSocketConnectionManager connectionManager)
    {
        _hubContext = hubContext;
        _connectionManager = connectionManager;
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

    public async Task SendNotificationAsync(string userId, string message)
    {
        await _connectionManager.BroadcastMessageAsync($"Notification: {message}");
    }

}
