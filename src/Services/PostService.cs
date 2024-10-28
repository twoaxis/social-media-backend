using MySql.Data.MySqlClient;

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
}