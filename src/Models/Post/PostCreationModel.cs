using System.ComponentModel.DataAnnotations;

namespace social_media_backend.Models.Post;

public class PostCreationModel
{
	[Required]
	public String content { get; set; }
    
}