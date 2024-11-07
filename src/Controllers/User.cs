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
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();
            
            try
            {
                var uid = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                
                UserProfile userProfile = _userService.GetUserProfile(username, uid);

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

        [HttpPost("{username}/unfollow")]
        public IActionResult UnfollowUser(string username)
        {
            DatabaseService.OpenConnection();

            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var followerId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var followingId = _userService.GetUserIdByUsername(username);

                if (followerId == followingId)
                    return BadRequest(); // Cannot unfollow yourself

                bool success = _userService.UnfollowUser(followerId, followingId);
                if (!success)
                    return NotFound(); // You are not following this user.

                return Ok(); // Successfully unfollowed the user
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


        [HttpPost("edit")]
        public IActionResult Edit_User_Data([FromBody] UserData userData)
        {
	    
            if (userData.username == null && userData.bio == null && userData.password == null ) return BadRequest("no params sended.");

            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

	    if (TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]) < 1) return BadRequest("token is not valid.");
            try
            {
                
                var username = userData.username;
                var bio = userData.bio;
                var password = userData.password;
		
                var token = authorizationHeader.ToString().Split(" ")[1];
                var email = TokenUtil.GetEmailFromJwt(token);
                bool result = _userService.EditUserData(email,username,password,bio);
                if (result) {
                    return Ok("Data Modified Successfuly ..");
                }else {
                    return Conflict("User Already taken");
                }
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }


    }
}
