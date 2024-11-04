namespace social_media_backend.Models.Post;

[Serializable]
public class PostAuthor ( int id, string username, string name )
{
	public int id { get; set; } = id;
	public string username { get; set; } = username;
	public string name { get; set; } = name;
}