using Microsoft.AspNetCore.Mvc;
using social_media_backend.Exceptions.Auth;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;
using social_media_backend.src.Services;
using social_media_backend.Util;
namespace social_media_backend.Controllers
{
    [ApiController]
    [Route("/friends")]
    public class FriendController : ControllerBase
    {
        private readonly FriendService _friendService = new();
        private readonly UserService _userService = new();


        [HttpPut("{username}")]
        public IActionResult SendFriendRequest(string username)
        {
            DatabaseService.OpenConnection();
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var targetUserId = _userService.GetUserIdByUsername(username); 

                _friendService.SendFriendRequest(userId, targetUserId);
                return Ok();
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{username}/accept")]
        public IActionResult AcceptFriendRequest(string username)
        {
            DatabaseService.OpenConnection();
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var requesterId = _userService.GetUserIdByUsername(username);

                _friendService.AcceptFriendRequest(userId, requesterId);
                return Ok(new { message = "Friend request accepted." });
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("{username}/reject")]
        public IActionResult RejectFriendRequest(string username)
        {
            DatabaseService.OpenConnection();
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var requesterId = _userService.GetUserIdByUsername(username);

                _friendService.RejectFriendRequest(userId, requesterId);
                return Ok();
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
            }
            catch (UserNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult GetFriends()
        {
            DatabaseService.OpenConnection();
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var friends = _friendService.GetFriends(userId);

                return Ok(friends);
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
            }
        }

        [HttpGet("requests")]
        public IActionResult GetFriendRequests()
        {
            DatabaseService.OpenConnection();
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var requests = _friendService.GetFriendRequests(userId);

                return Ok(requests);
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
            }
        }
    }
}
