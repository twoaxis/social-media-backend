using Microsoft.AspNetCore.Mvc;
using social_media_backend.Exceptions;
using social_media_backend.Models.Auth;
using social_media_backend.Services;

namespace social_media_backend.Controllers
{
    
	[Route("/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _authService = new();

		[HttpPost("signup")]
		public IActionResult Signup([FromBody] SignupRequestModel signupRequestModel)
		{
			try
			{
				var token = _authService.Signup(signupRequestModel.username, signupRequestModel.name, signupRequestModel.email, signupRequestModel.password);
                
				return Ok(new {
					token
				});
			}
			catch (UserExistsException)
			{
				return Conflict();
			}
		}
        
		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequestModel loginRequestModel)
		{
			try
			{
				var token = _authService.Login(loginRequestModel.email, loginRequestModel.password);
                
				return Ok(new {
					token
				});
			}
			catch (InvalidCredentialsException)
			{
				return Unauthorized();
			}
		}
	}
    
}