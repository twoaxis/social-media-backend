using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using social_media_backend.Services;
using social_media_backend.Util;

namespace social_media_backend.src.Controllers
{
    [Route("/notifications")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService = new();

        [HttpGet]
        public IActionResult GetNotifications()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader)) return Unauthorized();
            if (!authorizationHeader.ToString().StartsWith("Bearer ")) return Unauthorized();

            try
            {
                var userId = TokenUtil.ValidateToken(authorizationHeader.ToString().Split(" ")[1]);

                var notifications = _notificationService.GetNotifications(userId);

                return Ok(notifications);
            }
            catch (SecurityTokenException)
            {
                return Unauthorized();
            }
        }
    }
}
