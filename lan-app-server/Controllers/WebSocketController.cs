using lan_app_server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace lan_app_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : Controller
    {
        private readonly WebSocketConnectionManager _webSocketConnectionManager;
        private readonly ITokenManager _tokenValidator;

        public WebSocketController(WebSocketConnectionManager webSocketConnectionManager, JwtTokenHandler jwtTokenValidator)
        {
            _webSocketConnectionManager = webSocketConnectionManager;
            _tokenValidator = jwtTokenValidator;
        }

        [HttpGet("connect")]
        public async Task ConnectWebSocket(string token)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                if (!_tokenValidator.ValidateToken(token, out var username))
                    return;

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _webSocketConnectionManager.AddConnection(username, webSocket);

                var payLoad = JsonSerializer.Serialize(new
                {
                    Type = "BroadcastMessage",
                    Message = username + " joined!"
                });
                await _webSocketConnectionManager.BroadcastMessage(payLoad);
                await ReceiveMessage(webSocket, username);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task ReceiveMessage(WebSocket webSocket, string username)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // Handle the incoming message here (e.g., broadcast or respond)
                    SocketPayload msg = JsonSerializer.Deserialize<SocketPayload>(message);
                    if (msg.Type == "OneToOne")
                    {
                        var payLoad = JsonSerializer.Serialize(new
                        {
                            Type = "PersonalMessage",
                            Message = username + "(P) - " + msg.Message
                        });
                        await _webSocketConnectionManager.SendMessageToUser(msg.ToUser, payLoad);
                    }
                    else if (msg.Type == "Broadcast")
                    {
                        var payLoad = JsonSerializer.Serialize(new
                        {
                            Type = "BroadcastMessage",
                            Message = username + " - " + msg.Message
                        });
                        await _webSocketConnectionManager.BroadcastMessage(payLoad);
                    }
                    
                }
            }

            var payLoad1 = JsonSerializer.Serialize(new
            {
                Type = "BroadcastMessage",
                Message = username + " left!"
            });
            await _webSocketConnectionManager.BroadcastMessage(payLoad1);
            _webSocketConnectionManager.RemoveConnection(username, webSocket);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

    }
}
