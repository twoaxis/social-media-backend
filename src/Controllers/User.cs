﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using social_media_backend.Exceptions;
using social_media_backend.Exceptions.Auth;
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
            catch (InvalidCredentialsException)
            {
                return Unauthorized();
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
            catch (InvalidTokenException)
            {
                return Unauthorized();
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
                    return Conflict(); // You are not following this user.

                return Ok(); // Successfully unfollowed the user
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
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
        public IActionResult EditProfile([FromBody] UpdateUserModel updateUserModel)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();
            
            try
            {

                var id = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
                
                var username = updateUserModel.username;
                var bio = updateUserModel.bio;
		
                _userService.EditUserData(id, username, bio);

                return Ok();

            }
            catch (UserExistsException)
            {
                return Conflict(new
                {
                    code = "auth/username-taken"
                });
            }
            catch (InvalidTokenException)
            {
                return Unauthorized();
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }

        [HttpGet("search")]
        public IActionResult SearchUsers([FromQuery] string query)
        {
            var users = _userService.SearchUsers(query);
            
            return Ok(users);
        }

    }
}
