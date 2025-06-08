using System.Net.WebSockets;
using System.Text;

public class WebSocketConnectionManager
{
    private readonly Dictionary<string, WebSocket> _connections = new();

    public void AddConnection(string userId, WebSocket webSocket)
    {
        _connections[userId] = webSocket;
    }

    public void RemoveConnection(string userId)
    {
        if (_connections.TryGetValue(userId, out var socket))
        {
            socket.Dispose();
            _connections.Remove(userId);
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
