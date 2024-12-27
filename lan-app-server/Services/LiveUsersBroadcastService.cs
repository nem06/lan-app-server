using System.Text.Json;

namespace lan_app_server.Services
{
    public class LiveUsersBroadcastService : BackgroundService
    {
        private readonly WebSocketConnectionManager _webSocketConnectionManager;

        public LiveUsersBroadcastService(WebSocketConnectionManager webSocketConnectionManager)
        {
            _webSocketConnectionManager = webSocketConnectionManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var liveUsers = _webSocketConnectionManager.GetAllConnectedUsers();
                var message = JsonSerializer.Serialize(new
                {
                    Type = "LiveUsers",
                    Users = liveUsers
                });

                await _webSocketConnectionManager.BroadcastMessage(message);

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
