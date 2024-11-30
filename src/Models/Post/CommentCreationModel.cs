using System.ComponentModel.DataAnnotations;

namespace social_media_backend.Models.Post;

public class CommentCreationModel
{
    [Required]
    public String Content { get; set; }

}
