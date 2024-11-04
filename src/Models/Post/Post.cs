namespace social_media_backend.Models.Post;

[Serializable]
public class Post (int id, string content, DateTime createdAt, PostAuthor author)
{
	public int id { get; set; } = id;
	public string content { get; set; } = content;
	public DateTime createdAt { get; set; } = createdAt;
	public PostAuthor author { get; set; } = author;
}