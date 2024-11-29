using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using social_media_backend.Models.Post;
using social_media_backend.Services;
using social_media_backend.Util;

namespace social_media_backend.src.Controllers
{
	[Route("/posts")]
	[ApiController]
	public class PostController : ControllerBase
	{
		
		private readonly PostService _postService = new();
		
		[HttpPut]
		public IActionResult CreatePost([FromBody] PostCreationModel postCreationModel)
		{
			if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
			if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

			try
			{
				var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
				
				var postId = _postService.CreatePost(userId, postCreationModel.content);
			
				return Ok(new
				{
					postId
				});
			}
			catch (SecurityTokenException e)
			{
				return Unauthorized();
			}
		}

		[HttpGet]
		public IActionResult GetHomePosts()
		{
			if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
			if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

			try
			{
				var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);
				
				var posts = _postService.GetHomePagePosts(userId);

				return Ok(posts);
			}
			catch (SecurityTokenException e)
			{
				return Unauthorized();
			}
		}


        [HttpPost("{postId}/like")]
        public IActionResult LikePost(int postId)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);

                _postService.LikePost(userId, postId);

                return Ok();
            }
            catch (SecurityTokenException)
            {
                return Unauthorized();
            }
            catch (InvalidOperationException)
            {
                return Conflict();
            }
        }

        [HttpPut("{postId}/comment")]
        public IActionResult AddComment(int postId, [FromBody] CommentCreationModel commentModel)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);

                var commentId = _postService.AddComment(userId, postId, commentModel.Content);

                return Ok(new
                {
                    commentId
                });
            }
            catch (SecurityTokenException)
            {
                return Unauthorized();
            }
        }
    }
}