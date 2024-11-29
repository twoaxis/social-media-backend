namespace social_media_backend.Models.Post;

[Serializable]
public class Post (int id, string content, DateTime createdAt, PostAuthor author, int likeCount, bool isLiked, List<Comment> comments)
{
	public int id { get; set; } = id;
	public string content { get; set; } = content;
	public DateTime createdAt { get; set; } = createdAt;
	public PostAuthor author { get; set; } = author;
    public int likeCount { get; set; } = likeCount;
    public bool isLiked { get; set; } = isLiked;
    public List<Comment> comments { get; set; } = comments;
}