namespace lan_app_server.Models
{
    public class SocketPayload
    {
        public string Type { get; set; }
        public int ToUser {  get; set; }
        public int FromUser { get; set; }
        public string Message {  get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
