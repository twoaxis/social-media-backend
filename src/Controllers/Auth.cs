using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using social_media_backend.Exceptions;
using social_media_backend.Exceptions.Auth;
using social_media_backend.Models.Auth;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;
using social_media_backend.Util;

namespace social_media_backend.Controllers
{
    
	[Route("/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly AuthService _authService = new();
		private readonly CodeService _codeService = new();

		[HttpPost("signup")]
		public IActionResult Signup([FromBody] SignupRequestModel signupRequestModel)
		{
			try
			{
				var userId = _authService.Signup(signupRequestModel.username, signupRequestModel.name, signupRequestModel.email, signupRequestModel.password);
				
				var sessionId = _codeService.CreateNewEmailVerificationCode(signupRequestModel.email, userId);
				
				return Ok(new {
					sessionId
				});
			}
			catch (UserExistsException)
			{
				return Conflict(new
				{
					code = "auth/username-taken"
				});
			}
			catch (EmailTakenException)
			{
				return Conflict(new
				{
					code = "auth/email-taken"
				});
			}
		}
		
		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequestModel loginRequestModel)
		{
			try
			{
				var token = _authService.Login(loginRequestModel.email, loginRequestModel.password);
				
				return Ok(new
				{
					status = "complete",
					token
				});
			}
			catch (InvalidCredentialsException)
			{
				return Unauthorized();
			}
			catch (UserNotVerifiedException e)
			{
				var sessionId = _codeService.CreateNewEmailVerificationCode(loginRequestModel.email, e.userId);
				return Ok(new
				{
					status = "email-verification",
					sessionId
				});
			}
		}
		[HttpPost("verifyemail")]
		public IActionResult VerifyEmail([FromBody] EmailVerificationRequestModel emailVerificationRequestModel)
		{
			try
			{
				var uid = _codeService.VerifyCode(emailVerificationRequestModel.sessionId, emailVerificationRequestModel.code);
				
				var token = _authService.VerifyMail(uid);

				return Ok(new
				{
					token
				});
			}
			catch (InvalidVerificationCodeException)
			{
				return Unauthorized();
			}
		}

		[HttpPost("logout")]
        public IActionResult Logout([FromBody] string token)
        {
            try
            {
	            TokenUtil.ValidateToken(token);

                _authService.Logout(token);
                
                return Ok();
            }
            catch (SecurityTokenException e)
            {
	            return Unauthorized();
            }
        }
	}
    
}