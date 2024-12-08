using MySql.Data.MySqlClient;
using social_media_backend.Exceptions;
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

                using var command = new MySqlCommand(@"SELECT u.id, u.username, u.name, u.bio, (SELECT COUNT(*) FROM follows WHERE follower_id = u.id) AS following_count, (SELECT COUNT(*) FROM follows WHERE following_id = u.id) AS follower_count,(SELECT COUNT(*) FROM follows WHERE follower_id = @followerId AND following_id = @followingId) AS isFollowing FROM users u WHERE u.username = @username;",
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
                        reader.IsDBNull(reader.GetOrdinal("bio")) ? null : reader.GetString("bio"),
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
        


        public void EditUserData(int id, string? username = null, string? bio = null)
        {
            DatabaseService.OpenConnection();

            if (username != null && DoesUserExistByUsername(username)) { 
                throw new UserExistsException();
            }

            try
            {
                using var command = new MySqlCommand("UPDATE users SET username=COALESCE(@username, username), bio=@bio WHERE id = @id", DatabaseService.Connection);
                command.Parameters.AddWithValue("@id", id);
                
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@bio", bio);
                command.ExecuteNonQuery();
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
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

                    //Create Notification
                    var notificationService = new NotificationService();
                    string username = GetUsernameById(followerId);
                    notificationService.CreateNotification(followingId, "New Follower", $"{username} started following you.");


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

        public UserSearchModel[] SearchUsers(string searchQuery)
        {
            List<UserSearchModel> users = [];
            DatabaseService.OpenConnection();

            try
            {
                using var command = new MySqlCommand("SELECT id, username, name FROM users WHERE name LIKE @query OR username LIKE @query",
                    DatabaseService.Connection);
                command.Parameters.AddWithValue("@query", $"%{searchQuery}%");

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new UserSearchModel(
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("name")
                    ));
                }
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
            
            return users.ToArray();
        }
        private string GetUsernameById(int userId)
        {
            using var command = new MySqlCommand("SELECT username FROM users WHERE id = @userId;", DatabaseService.Connection);
            command.Parameters.AddWithValue("@userId", userId);

            var result = command.ExecuteScalar();
            if (result != null)
            {
                return result.ToString();
            }
            else
            {
                throw new InvalidOperationException("User not found.");
            }
        }
    }
}
