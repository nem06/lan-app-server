namespace lan_app_server.Models
{
    public class SocketPayload
    {
        public string Type { get; set; }
        public string ToUser {  get; set; }
        public string FromUser { get; set; }
        public string Message {  get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
