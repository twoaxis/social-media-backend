using System.ComponentModel.DataAnnotations;

namespace social_media_backend.Models.Auth;

public class LoginRequestModel
{
    
    [Required]
    [EmailAddress]
    public String email { get; set; }
    
    [Required]
    public String password { get; set; }
}
