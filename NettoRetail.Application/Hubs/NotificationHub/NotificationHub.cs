using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace NettoRetail.Application.Hubs.NotificationHub;

public sealed class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var shopId = user?.FindFirst("shop_id")?.Value;
        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(shopId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"shop:{shopId}");
        }

        await base.OnConnectedAsync();
    }

    public async Task NotifyAll(string message)
    {
        await Clients.All.ReceiveNotification(message);
    }

    public async Task NotifyGroup(string groupName, string message)
    {
        await Clients.Group(groupName).ReceiveNotification(message);
    }

    public async Task NotifyClient(string clientId, string message)
    {
        await Clients.User(clientId).ReceiveNotification(message);
    }
}