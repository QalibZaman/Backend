using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NettoRetail.Application.Hubs.NotificationHub;

[Authorize]
public sealed class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var callerClaims = Context.User;
        var shopId = callerClaims?.FindFirst("shop_id")?.Value;

        if (!string.IsNullOrEmpty(shopId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"shop:{shopId}");
        }

        await base.OnConnectedAsync();
    }
}