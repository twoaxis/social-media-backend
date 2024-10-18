using Microsoft.AspNetCore.Mvc;

namespace opensource_backend_api.Controllers
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