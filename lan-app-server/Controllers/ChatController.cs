using lan_app_server.Models;
using lan_app_server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace lan_app_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        [HttpGet("GetAllUsers")]
        //[ValidateAntiForgeryToken]
        public ActionResult GetAllUsers()
        {
            string allUsers = PostgreSQLService.GetJsonFromPostGre("GetAllUsers", null);
            return Ok(allUsers);
        }
    }
}
