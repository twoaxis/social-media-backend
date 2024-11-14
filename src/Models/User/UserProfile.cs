namespace social_media_backend.Models.User;

using social_media_backend.Models.Post;

[Serializable]
public class UserProfile(int id, string username, string name, string? bio, int followerCount, int followingCount, bool isFollowing, Post[] posts)
{
	public int id { get; set; } = id;
	public string username { get; set; } = username;
	public string name { get; set; } = name;
	public string? bio { get; set; } = bio;
	public int followerCount { get; set; } = followerCount;
	public int followingCount { get; set; } = followingCount;
	public bool isFollowing { get; set; } = isFollowing;
	public Post[] posts { get; set; }= posts;
}