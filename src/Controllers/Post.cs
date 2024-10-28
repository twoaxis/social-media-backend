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
				var creds = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);

				if (!int.TryParse(creds.Claims.ToArray()[0].Value, out var userId))
					return Problem();
					
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
	}
}