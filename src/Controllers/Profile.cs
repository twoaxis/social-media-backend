using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using social_media_backend.Services;

namespace social_media_backend.src.Controllers
{
    [Route("/profile")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private string GetUserProfile(string username)
        {
            DatabaseService.OpenConnection();

            try
            {
                using (var command = new MySqlCommand("SELECT username FROM users WHERE username = @username", DatabaseService.Connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString("username");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                DatabaseService.CloseConnection();
            }
            return null;
        }
    }
}
