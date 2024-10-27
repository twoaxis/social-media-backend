using MySql.Data.MySqlClient;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;

namespace social_media_backend.src.Services
{
    public class UserService
    {
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
        private static bool DoesUserExistByUsername(string username)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", DatabaseService.Connection);
            command.Parameters.AddWithValue("@username", username);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
        private static bool DoesUserExistByEmail(string email)
        {
            using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", DatabaseService.Connection);
            command.Parameters.AddWithValue("@email", email);
            var result = command.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }
    }
}
