using lan_app_server.Models;
using Microsoft.AspNetCore.Mvc;

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
            var token = _tokenManager.GenerateToken(user.name);
            return Ok(new { success = true, token });
        }
    }
}
