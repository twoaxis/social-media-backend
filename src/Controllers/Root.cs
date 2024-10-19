using Microsoft.AspNetCore.Mvc;

namespace social_media_backend.Controllers
{
    [Route("/")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Get()
        {
            return Ok("Pong!");
        }
    }
    
}