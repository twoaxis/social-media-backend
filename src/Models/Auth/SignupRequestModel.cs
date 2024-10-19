using System.ComponentModel.DataAnnotations;

namespace social_media_backend.Models.Auth;

public class SignupRequestModel
{
    [Required]
    public String name { get; set; }
    
    [Required]
    public String username { get; set; }
    
    [Required]
    [EmailAddress]
    public String email { get; set; }
    
    [Required]
    public String password { get; set; }
}