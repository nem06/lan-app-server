using lan_app_server.Models;
using lan_app_server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace lan_app_server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogInController : Controller
    {
        private readonly ITokenManager _tokenManager;

        public LogInController(JwtTokenHandler jwtTokenValidator)
        {
            _tokenManager = jwtTokenValidator;
        }

        [HttpPost("LogIn")]
        //[ValidateAntiForgeryToken]
        public ActionResult LogIn([FromBody] User user)
        {
            string result = PostgreSQLService.GetJsonFromPostGre("CheckLogin", JsonConvert.SerializeObject(user));

            if(result == null)
            {
                return BadRequest("Invalid login attempt. Please check your credentials.");
            }

            User loggedinUser = JsonConvert.DeserializeObject<User>(result);
            var token = _tokenManager.GenerateToken(loggedinUser.user_id.ToString());
            return Ok(new { success = true, token,  loggedinUser.user_id, loggedinUser.user_name, loggedinUser.first_name, loggedinUser.last_name, loggedinUser.about});
        }
    }
}
