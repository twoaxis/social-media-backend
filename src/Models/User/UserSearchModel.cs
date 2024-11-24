namespace social_media_backend.Models.User;

[Serializable]
public class UserSearchModel(int id, string username, string name)
{
	public int id { get; set; } = id;
	public string username { get; set; } = username;
	public string name { get; set; } = name;
}