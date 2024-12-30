using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketConnectionManager
{
    private ConcurrentDictionary<int, List<WebSocket>> _connections = new();

    public void AddConnection(int user_id, WebSocket webSocket)
    {
        if (!_connections.ContainsKey(user_id))
            _connections[user_id] = new List<WebSocket>();
        _connections[user_id].Add(webSocket);
    }

    public void RemoveConnection(int user_id, WebSocket webSocket)
    {
        _connections.TryGetValue(user_id, out var webSockets);
        webSockets.Remove(webSocket);
        if (webSockets.Count == 0)
            _connections.TryRemove(user_id, out webSockets);
    }


    public List<WebSocket> GetConnection(int user_id)
    {
        _connections.TryGetValue(user_id, out var webSockets);
        return webSockets;
    }

    public List<int> GetAllConnectedUsers()
    {
        return _connections.Keys.ToList();
    }

    public async Task SendMessageToUser(int user_id, string message)
    {
        if (_connections.TryGetValue(user_id, out var webSockets))
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

    public async Task SendMessageToSelf(int user_id, string message, WebSocket webSocket)
    {
        if (_connections.TryGetValue(user_id, out var webSockets))
        {
            foreach (WebSocket ws in webSockets)
            {
                if (ws != webSocket && ws.State == WebSocketState.Open)
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                    var segment = new ArraySegment<byte>(buffer);
                    await ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
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
