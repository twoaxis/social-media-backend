using Microsoft.AspNetCore.Mvc;
using social_media_backend.src.Services;
using social_media_backend.src.Exceptions;

namespace social_media_backend.src.Controllers
{
    [Route("/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService = new();

        // HTTP GET endpoint to retrieve the user profile
        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            try
            {
                var userProfile = _userService.GetUserProfile(username);

                return Ok(new { username = userProfile.UserName, name = userProfile.Name }); 
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
