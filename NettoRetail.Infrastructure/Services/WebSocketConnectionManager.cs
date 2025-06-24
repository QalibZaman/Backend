using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public void AddConnection(string userId, WebSocket webSocket)
    {
        _connections[userId] = webSocket;
    }

    public void RemoveConnection(string userId)
    {
        if (_connections.TryRemove(userId, out var socket))
        {
            socket.Dispose();
        }
    }

    public async Task BroadcastMessageAsync(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        foreach (var webSocket in _connections.Values)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(messageBytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
