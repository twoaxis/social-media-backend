using MySql.Data.MySqlClient;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;

namespace social_media_backend.src.Services
{
    public class UserService
    {
        /*
         * TODO: Should have a model created for it.
         * - Create a model that represents a full user called `UserProfile`.
         */
        public (string UserName, string Name) GetUserProfile(string username)
        {
            DatabaseService.OpenConnection();

            if (!DoesUserExistByUsername(username))
            {
                DatabaseService.CloseConnection();
                throw new UserNotFoundException();
            }

            using (var command = new MySqlCommand("SELECT username, name FROM users WHERE username = @username", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (reader.GetString("username"), reader.GetString("name"));
                    }
                }
            }
            DatabaseService.CloseConnection();
            return (null, null);
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

            using (var command = new MySqlCommand("INSERT IGNORE INTO follows (follower_id, following_id) VALUES (@followerId, @followingId)", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@followerId", followerId);
                command.Parameters.AddWithValue("@followingId", followingId);
                int result = command.ExecuteNonQuery();

                DatabaseService.CloseConnection();
                return result > 0;
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
    }
}
