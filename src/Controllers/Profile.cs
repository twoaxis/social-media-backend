using Microsoft.AspNetCore.Mvc;
using social_media_backend.src.Services;
using social_media_backend.src.Exceptions;

namespace social_media_backend.src.Controllers
{
    [Route("/profile/{username}")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly ProfileService _profileService = new();

        // HTTP GET endpoint to retrieve the user profile
        [HttpGet()]
        public IActionResult GetProfile(string username)
        {
            try
            {
                var userProfile = _profileService.GetUserProfile(username);

                return Ok(new { username = userProfile.UserName, name = userProfile.Name }); 
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
