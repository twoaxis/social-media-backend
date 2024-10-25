using MySql.Data.MySqlClient;
using social_media_backend.Services;
using social_media_backend.src.Exceptions;

namespace social_media_backend.src.Services
{
    public class ProfileService
    {
        public (string UserName, string Name) GetUserProfile(string username)
        {
            DatabaseService.OpenConnection();

            if (!DoesProfileExists(username))
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
        private bool DoesProfileExists(string username)
        {
            using (var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@username", username);
                var result = command.ExecuteScalar();

                if (result != null && int.TryParse(result.ToString(), out int count))
                {
                    Console.WriteLine(count);
                    return count > 0; 
                }

                return false;
            }
        }

    }
}
