using MySql.Data.MySqlClient;
using social_media_backend.Models.Post;
using social_media_backend.Models.User;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;
using social_media_backend.Util;

namespace social_media_backend.src.Services
{
    public class UserService
    {
        private readonly PostService _postService = new();
        
        public UserProfile GetUserProfile(string username, int followingId)
        {
            UserProfile profile;
            DatabaseService.OpenConnection();

            try
            {
                int followerId = GetUserIdByUsername(username);
                if (!DoesUserExistByUsername(username))
                {
                    throw new UserNotFoundException();
                }

                using var command = new MySqlCommand(@"SELECT u.id, u.username, u.name, (SELECT COUNT(*) FROM follows WHERE follower_id = u.id) AS following_count, (SELECT COUNT(*) FROM follows WHERE following_id = u.id) AS follower_count,(SELECT COUNT(*) FROM follows WHERE follower_id = @followerId AND following_id = @followingId) AS isFollowing FROM users u WHERE u.username = @username;",
                    DatabaseService.Connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@followingId", followingId);
                command.Parameters.AddWithValue("@followerId", followerId);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    profile = new UserProfile(
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("name"),
                        reader.GetInt32("follower_count"),
                        reader.GetInt32("following_count"),
                        reader.GetBoolean("isFollowing"),
                        []
                    );
                    reader.Close();
                }
                else throw new UserNotFoundException();

                profile.posts = _postService.GetAllPosts(profile.id);
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
            return profile;
        }
        public bool DoesUserExistByUsername(string username)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", DatabaseService.Connection);
            command.Parameters.AddWithValue("@username", username);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
        public bool DoesUserExistByEmail(string email)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", DatabaseService.Connection);
            command.Parameters.AddWithValue("@email", email);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
        


        public bool EditUserData(string? email=null , string? username=null , string? password=null , string? bio=null )
        {
            DatabaseService.OpenConnection();

            if (username != null){
                if (DoesUserExistByUsername(username)){
                        return false;
                }
                else {
                    using var command = new MySqlCommand("UPDATE users SET username=@username WHERE email = @email", DatabaseService.Connection);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@username", username);
                    int result = command.ExecuteNonQuery();
                    return result > 0;

            }
            }

            if (password != null){
                var hashed_password = HashUtil.GenerateSHA256Hash(password);
                using var command = new MySqlCommand("UPDATE users SET password=@password WHERE email = @email", DatabaseService.Connection);
                command.Parameters.AddWithValue("@password", hashed_password);
                command.Parameters.AddWithValue("@email", email);
                int result = command.ExecuteNonQuery();
                return result > 0;
            }

            if (bio != null){
                using var command = new MySqlCommand("UPDATE users SET bio=@bio WHERE email = @email", DatabaseService.Connection);
                command.Parameters.AddWithValue("@bio", bio);
                command.Parameters.AddWithValue("@email", email);
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
	    DatabaseService.CloseConnection();
	    return false;
        }




        public bool DoesUserExistById(int id)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE id = @id", DatabaseService.Connection);
            command.Parameters.AddWithValue("@id", id);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        public bool FollowUser(int followerId, int followingId)
        {
            DatabaseService.OpenConnection();
            try
            {
                using (var command = new MySqlCommand("INSERT IGNORE INTO follows (follower_id, following_id) VALUES (@followerId, @followingId)", DatabaseService.Connection))
                {
                    command.Parameters.AddWithValue("@followerId", followerId);
                    command.Parameters.AddWithValue("@followingId", followingId);
                    int result = command.ExecuteNonQuery();

                    return result > 0;
                }
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }

        public int GetUserIdByUsername(string username)
        {
            using (var command = new MySqlCommand("SELECT id FROM users WHERE username = @username", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@username", username);
                var result = command.ExecuteScalar();
                if (result != null)
                    return Convert.ToInt32(result);

                throw new UserNotFoundException();
            }
        }


        public List<Dictionary<string, object>> GetFollowers(string username)
        {
            DatabaseService.OpenConnection();
            int userId = GetUserIdByUsername(username);

            var followers = new List<Dictionary<string, object>>();

            using (var command = new MySqlCommand("SELECT u.id, u.username, u.name FROM follows f JOIN users u ON f.follower_id = u.id WHERE f.following_id = @userId", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new Dictionary<string, object>
                        {
                            { "id", reader.GetInt32("id") },
                            { "Username", reader.GetString("username") },
                            { "Name", reader.GetString("name") }
                        };
                        followers.Add(user);
                    }
                }
            }

            DatabaseService.CloseConnection();
            return followers;
        }

        public List<Dictionary<string, object>> GetFollowing(string username)
        {
            DatabaseService.OpenConnection();
            int userId = GetUserIdByUsername(username);

            var following = new List<Dictionary<string, object>>();

            using (var command = new MySqlCommand("SELECT u.id, u.username, u.name FROM follows f JOIN users u ON f.following_id = u.id WHERE f.follower_id = @userId", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new Dictionary<string, object>
                        {
                            { "id", reader.GetInt32("id") },
                            { "Username", reader.GetString("username") },
                            { "Name", reader.GetString("name") }
                        };
                        following.Add(user);
                    }
                }
            }

            DatabaseService.CloseConnection();
            return following;
        }

        public bool UnfollowUser(int followerId, int followingId)
        {
            DatabaseService.OpenConnection();
            try
            {
                using (var command = new MySqlCommand("DELETE FROM follows WHERE follower_id = @followerId AND following_id = @followingId", DatabaseService.Connection))
                {
                    command.Parameters.AddWithValue("@followerId", followerId);
                    command.Parameters.AddWithValue("@followingId", followingId);
                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
        }


    }
}
