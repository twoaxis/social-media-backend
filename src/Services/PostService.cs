using MySql.Data.MySqlClient;
using social_media_backend.Models.Post;

namespace social_media_backend.Services;

public class PostService
{
	public int CreatePost(int userId, string content)
	{
		try
		{
			DatabaseService.OpenConnection();
			
			using var command = new MySqlCommand("INSERT INTO posts (author, content) VALUES (@author, @content); SELECT LAST_INSERT_ID();",
				DatabaseService.Connection);
			command.Parameters.AddWithValue("@author", userId);
			command.Parameters.AddWithValue("@content", content);

			var result = command.ExecuteScalar();
			var postId = Convert.ToInt32(result);
			return postId;
		}
		finally
		{
			DatabaseService.CloseConnection();
		}
	}

	public Post[] GetAllPosts(int userId)
	{
		List<Post> posts = [];
		DatabaseService.OpenConnection();

		try
		{
			using var command = new MySqlCommand("SELECT posts.id, posts.content, posts.createdAt, posts.createdAt, users.id AS uid, users.username, users.name FROM posts JOIN users ON posts.author = users.id WHERE users.id = @id ORDER BY posts.createdAt DESC;", DatabaseService.Connection);
			command.Parameters.AddWithValue("@id", userId);
			
			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				posts.Add(new Post(
					reader.GetInt32("id"),
					reader.GetString("content"),
					reader.GetDateTime("createdAt"),
					new PostAuthor(
						reader.GetInt32("uid"),
						reader.GetString("username"),
						reader.GetString("name")
					)
				));
			}
		}
		finally
		{
			DatabaseService.CloseConnection();
		}
		
		return posts.ToArray();
	}
}