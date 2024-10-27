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

		 [HttpPost("logout")]
        public IActionResult Logout([FromBody] string token)
        {
            try
            {
                var success = _authService.Logout(token);
                
                if (!success)
                    return Unauthorized(new { message = "Invalid token or user not found." });
                
                return Ok(new { message = "Successfully logged out." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while logging out.", error = ex.Message });
            }
        }
	}
    
}