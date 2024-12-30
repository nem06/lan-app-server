using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public interface ITokenManager
{
    string GenerateToken(string username);
    bool ValidateToken(string token, out int user_id);
}

public class JwtTokenHandler : ITokenManager
{
    private readonly string _key;

    public JwtTokenHandler(IConfiguration configuration)
    {
        _key = "YourSuperSecretKey12345!@#20241211";
    }

    public string GenerateToken(string user_id)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user_id), // Standard subject claim
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier for the token
                    new Claim("user_id", user_id) // Custom claim
                }),
            //Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token, out int user_id)
    {
        user_id = 0;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            // Validate the token
            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false, // Optionally validate Issuer
                ValidateAudience = false, // Optionally validate Audience
                ClockSkew = TimeSpan.Zero // No tolerance for token expiration
            }, out var validatedToken);

            string uid = claimsPrincipal.FindFirst("user_id")?.Value;
            user_id = int.Parse(uid);
            return true;
        }
        catch
        {
            return false;
        }
    }
}


//using System.Net.WebSockets;
//using System.Text;

//namespace lan_app_server.Models
//{
//    public class WebSocketHandler
//    {
//        private readonly WebSocketConnectionManager _connectionManager;

//        public WebSocketHandler(WebSocketConnectionManager connectionManager)
//        {
//            _connectionManager = connectionManager;
//        }
//        public static async Task HandleWebSocket(WebSocket socket)
//        {
//            var buffer = new byte[1024 * 4]; // Buffer for receiving messages

//            while (socket.State == WebSocketState.Open)
//            {
//                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

//                if (result.MessageType == WebSocketMessageType.Close)
//                {
//                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
//                    break;
//                }

//                if (result.MessageType == WebSocketMessageType.Text)
//                {
//                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
//                    Console.WriteLine($"Message received: {receivedMessage}");

//                    // Echo the message back to the client
//                    var responseMessage = $"Server: {receivedMessage}";
//                    var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
//                    await socket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
//                }
//            }
//        }
//    }
//}