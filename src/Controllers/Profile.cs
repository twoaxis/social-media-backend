using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using social_media_backend.Services;

namespace social_media_backend.src.Controllers
{
    [Route("/profile/{username}")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private (string UserName,string Name) GetUserProfile(string username)
        {
            DatabaseService.OpenConnection();

            try
            {
                using (var command = new MySqlCommand("SELECT username, name FROM users WHERE username = @username", DatabaseService.Connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (reader.GetString("username") , reader.GetString("name"));
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
            return (null,null);
        }
        // HTTP GET endpoint to retrieve the user profile
        [HttpGet()]
        public IActionResult GetProfile(string username)
        {
            try
            {
                var userProfile = GetUserProfile(username);

                if (userProfile.UserName == null)
                {
                    return NotFound(); 
                }

                return Ok(new { username = userProfile.UserName, name = userProfile.Name }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
