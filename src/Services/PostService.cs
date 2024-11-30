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
			using var command = new MySqlCommand("SELECT posts.id, posts.content, posts.createdAt, posts.createdAt, users.id AS uid, users.username, users.name, (SELECT COUNT(*) FROM post_likes WHERE post_likes.post_id = posts.id) AS likeCount, EXISTS (SELECT 1 FROM post_likes WHERE post_likes.post_id = posts.id AND post_likes.user_id = @userId) AS isLiked FROM posts JOIN users ON posts.author = users.id WHERE users.id = @id ORDER BY posts.createdAt DESC;", DatabaseService.Connection);
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
                    ),
                    reader.GetInt32("likeCount"),
                    reader.GetBoolean("isLiked"),
					[]
                ));
			}
            reader.Close();
            foreach (var post in posts)
            {
                post.comments = GetComments(post.id);
            }

        }
		finally
		{
			DatabaseService.CloseConnection();
		}
		
		return posts.ToArray();
	}

	public Post[] GetHomePagePosts(int userId)
	{
		List<Post> posts = [];
		DatabaseService.OpenConnection();

		try
		{
			using var command = new MySqlCommand("SELECT posts.id, posts.content, posts.createdAt, posts.createdAt, users.id AS uid, users.username, users.name, (SELECT COUNT(*) FROM post_likes WHERE post_likes.post_id = posts.id) AS likeCount, EXISTS (SELECT 1 FROM post_likes WHERE post_likes.post_id = posts.id AND post_likes.user_id = @id) AS isLiked FROM posts JOIN users ON posts.author = users.id JOIN follows ON follows.following_id = users.id WHERE follows.follower_id = @id ORDER BY posts.createdAt DESC;", DatabaseService.Connection);
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
					),
                    reader.GetInt32("likeCount"),
					reader.GetBoolean("isLiked"),
	               []
				));
			}
			reader.Close();
			foreach(var post in posts)
			{
				post.comments = GetComments(post.id);
			}
		}
		finally
		{
			DatabaseService.CloseConnection();
		}
		
		return posts.ToArray();
	}

    public void LikePost(int userId, int postId)
    {
        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand("INSERT INTO post_likes (user_id, post_id) VALUES (@userId, @postId);",
                DatabaseService.Connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@postId", postId);

            command.ExecuteNonQuery();
        }
        catch (MySqlException e) when (e.Number == 1062) // Duplicate entry error
        {
            throw new InvalidOperationException(); // "User already liked this post."
        }
        finally
        {
            DatabaseService.CloseConnection();
        }
    }
    public int AddComment(int userId, int postId, string content)
    {
        try
        {
            DatabaseService.OpenConnection();

            using var command = new MySqlCommand("INSERT INTO post_comments (user_id, post_id, content) VALUES (@userId, @postId, @content); SELECT LAST_INSERT_ID();",
                DatabaseService.Connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@postId", postId);
            command.Parameters.AddWithValue("@content", content);

            var result = command.ExecuteScalar();
            var commentId = Convert.ToInt32(result);
            return commentId;
        }
        finally
        {
            DatabaseService.CloseConnection();
        }
    }


    private List<Comment> GetComments(int postId)
    {
        List<Comment> comments = new();
        using var commentCommand = new MySqlCommand(@"SELECT post_comments.id, post_comments.content, post_comments.createdAt, users.id AS uid, users.username, users.name FROM post_comments JOIN users ON post_comments.user_id = users.id WHERE post_comments.post_id = @postId ORDER BY post_comments.createdAt ASC;", DatabaseService.Connection);

        commentCommand.Parameters.AddWithValue("@postId", postId);

        using var commentReader = commentCommand.ExecuteReader();
        while (commentReader.Read())
        {
            comments.Add(new Comment(
                commentReader.GetInt32("id"),
                commentReader.GetString("content"),
                commentReader.GetDateTime("createdAt"),
                new PostAuthor(
                    commentReader.GetInt32("uid"),
                    commentReader.GetString("username"),
                    commentReader.GetString("name")
                )
            ));
        }

        return comments;
    }

}