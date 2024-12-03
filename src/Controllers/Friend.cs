using Microsoft.AspNetCore.Mvc;
using social_media_backend.Exceptions;
using social_media_backend.Exceptions.Auth;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;
using social_media_backend.src.Services;
using social_media_backend.Util;
using social_media_backend.src.Services;
using Org.BouncyCastle.Asn1.Ocsp;
using social_media_backend.Models.Friend;
namespace social_media_backend.Controllers
{
    [ApiController]
    [Route("/friends")]
    public class FriendController : ControllerBase
    {
        private readonly FriendService _friendService = new();
        private readonly UserService _UserService = new();


        [HttpPut("{username}")]
        public IActionResult SendFriendRequest(string username)
        {
            DatabaseService.OpenConnection();
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                var targetUserId = _UserService.GetUserIdByUsername(username); // افترض وجود هذه الدالة

                _friendService.SendFriendRequest(userId, targetUserId);
                return Ok(new { message = "Friend request sent." });
            }
            catch (InvalidTokenException)
            {
                return Unauthorized(new { message = "Invalid token." });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "Target user not found." });
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
                var requesterId = _UserService.GetUserIdByUsername(username);

                _friendService.AcceptFriendRequest(userId, requesterId);
                return Ok(new { message = "Friend request accepted." });
            }
            catch (InvalidTokenException)
            {
                return Unauthorized(new { message = "Invalid token." });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "Requesting user not found." });
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
                var requesterId = _UserService.GetUserIdByUsername(username);

                _friendService.RejectFriendRequest(userId, requesterId);
                return Ok(new { message = "Friend request rejected." });
            }
            catch (InvalidTokenException)
            {
                return Unauthorized(new { message = "Invalid token." });
            }
            catch (UserNotFoundException)
            {
                return NotFound(new { message = "Requesting user not found." });
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
                return Unauthorized(new { message = "Invalid token." });
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
                return Unauthorized(new { message = "Invalid token." });
            }
        }
    }
}
