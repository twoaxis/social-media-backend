using Microsoft.AspNetCore.Mvc;
using social_media_backend.Models.User;
using social_media_backend.src.Services;
using social_media_backend.src.Exceptions;
using social_media_backend.Services;
using social_media_backend.Util;

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
                UserProfile userProfile = _userService.GetUserProfile(username);

                return Ok(userProfile); 
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }


        [HttpPost("{username}/follow")]
        public IActionResult FollowUser(string username)
        {

            DatabaseService.OpenConnection();

            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {

                var followerId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var followingId = _userService.GetUserIdByUsername(username);


                if (followerId == followingId)
                    return BadRequest(); //following yourself
                bool success = _userService.FollowUser(followerId, followingId);
                if (!success)
                    return Conflict(); //Already following the user

                return Ok(); //Successfully followed the user
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }

        [HttpGet("{username}/followers")]
        public IActionResult GetFollowers(string username)
        {
            try
            {
                var followers = _userService.GetFollowers(username);
                return Ok(followers);
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
        }

        [HttpGet("{username}/following")]
        public IActionResult GetFollowing(string username)
        {
            try
            {
                var following = _userService.GetFollowing(username);
                return Ok(following);
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "User not found." });
            }
        }


    }
}
