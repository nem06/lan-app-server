namespace lan_app_server.Models
{
    public class User
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string password { get; set; }  
        public string first_name { get; set; }
        public string last_name { get; set;}
        public string about { get; set; }   
    }
}
