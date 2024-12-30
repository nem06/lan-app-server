using lan_app_server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
                if (!_tokenValidator.ValidateToken(token, out var user_id))
                    return;

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _webSocketConnectionManager.AddConnection(user_id, webSocket);

                //var payLoad = JsonSerializer.Serialize(new
                //{
                //    Type = "BroadcastMessage",
                //    Message = username + " joined!"
                //});
                //await _webSocketConnectionManager.BroadcastMessage(payLoad);
                await ReceiveMessage(webSocket, user_id);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task ReceiveMessage(WebSocket webSocket, int user_id)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result;

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        // Handle the incoming message here (e.g., broadcast or respond)
                        SocketPayload msg = JsonSerializer.Deserialize<SocketPayload>(message);
                        if (msg.Type == "LivePing")
                        { }
                        else if (msg.Type == "OneToOne")
                        {
                            var payLoad = JsonSerializer.Serialize(new
                            {
                                Type = "OneToOne",
                                FromUser = user_id,
                                Message = msg.Message
                            });
                            if(user_id != msg.ToUser)
                                await _webSocketConnectionManager.SendMessageToUser(msg.ToUser, payLoad);
                            var payLoadSelf = JsonSerializer.Serialize(new
                            {
                                Type = "SelfCopy",
                                ToUser = msg.ToUser,
                                Message = msg.Message
                            });
                            await _webSocketConnectionManager.SendMessageToSelf(user_id, payLoadSelf, webSocket); // if multiple login
                        }
                        else if (msg.Type == "Broadcast")
                        {
                            var payLoad = JsonSerializer.Serialize(new
                            {
                                Type = "BroadcastMessage",
                                Message = user_id + " - " + msg.Message
                            });
                            await _webSocketConnectionManager.BroadcastMessage(payLoad);
                        }

                    }
                }
            }
            catch (WebSocketException ex)
            { }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                //var payLoad1 = JsonSerializer.Serialize(new
                //{
                //    Type = "BroadcastMessage",
                //    Message = user_id + " left!"
                //});
                //await _webSocketConnectionManager.BroadcastMessage(payLoad1);
                _webSocketConnectionManager.RemoveConnection(user_id, webSocket);
                if(webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);

                }
            }
            
        }

    }
}
