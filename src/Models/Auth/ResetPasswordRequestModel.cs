using System.ComponentModel.DataAnnotations;

namespace social_media_backend.Models.Auth;

public class ResetPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string email { get; set; }
}