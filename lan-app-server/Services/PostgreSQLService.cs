using Npgsql;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace lan_app_server.Services
{
    public static class PostgreSQLService
    {
        private static string connectionString {  get; set; }

        static PostgreSQLService() 
        {
            connectionString = "Host=localhost;Port=5432;Username=postgres;Password=naeem786;Database=lan-chat-app";
        }

        public static string GetJsonFromPostGre(string functionName, string inputJson)
        {
            string outputJson = null;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string cmndText = "";
                if (inputJson != null)
                    cmndText = "SELECT " + functionName + "(@input_json)";
                else
                    cmndText = "SELECT " + functionName + "()";

                using (var command = new NpgsqlCommand(cmndText, connection))
                {
                    if (inputJson != null)
                        command.Parameters.AddWithValue("input_json", NpgsqlTypes.NpgsqlDbType.Json, inputJson);

                    var result = command.ExecuteScalar().ToString();
                    outputJson = result == "" ? null : result;
                }

            }

            return outputJson; 
        }
    }
}
