using System.ComponentModel.DataAnnotations;

namespace social_media_backend.Models.Auth;

public class ResetPasswordUpdateRequest
{
    [Required] 
    public String sessionId { get; set; }

    [Required]
    public String code { get; set; }
    
    [Required]
    public String newPassword { get; set; }
}