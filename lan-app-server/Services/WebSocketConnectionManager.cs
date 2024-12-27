using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketConnectionManager
{
    private ConcurrentDictionary<string, List<WebSocket>> _connections = new();

    public void AddConnection(string connectionId, WebSocket webSocket)
    {
        if (!_connections.ContainsKey(connectionId))
            _connections[connectionId] = new List<WebSocket>();
        _connections[connectionId].Add(webSocket);
    }


    public void RemoveConnection(string connectionId, WebSocket webSocket)
    {
        _connections.TryGetValue(connectionId, out var webSockets);
        webSockets.Remove(webSocket);
        if (webSockets.Count == 0)
            _connections.TryRemove(connectionId, out webSockets);
    }


    public List<WebSocket> GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var webSockets);
        return webSockets;
    }

    public List<string> GetAllConnectedUsers()
    {
        return _connections.Keys.ToList();
    }

    public async Task SendMessageToUser(string connectionId, string message)
    {
        if (_connections.TryGetValue(connectionId, out var webSockets))
        {
            foreach(WebSocket webSocket in webSockets)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                    var segment = new ArraySegment<byte>(buffer);
                    await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }          
        }
    }


    public async Task BroadcastMessage(string message)
    {
        foreach (var webSockets in _connections.Values)
        {
            foreach(var webSocket in webSockets)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                    var segment = new ArraySegment<byte>(buffer);
                    await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }              
        }
    }
}
