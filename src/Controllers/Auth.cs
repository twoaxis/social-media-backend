using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using social_media_backend.Models.Auth;
using social_media_backend.Services;
using social_media_backend.Util;

namespace social_media_backend.Controllers
{
    
    [Route("/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequestModel signupRequestModel)
        {
            DatabaseService.OpenConnection();

            bool doesExist = false;
            using (var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username OR email = @email", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@username", signupRequestModel.username);
                command.Parameters.AddWithValue("@email", signupRequestModel.email);
                var result = command.ExecuteScalar();
                doesExist = Convert.ToInt32(result) > 0;
            }
            
            if (doesExist)
            {
                DatabaseService.CloseConnection();
                return Conflict();
            }

            int userId;
            using (var command = new MySqlCommand("INSERT INTO users (username, name, email, password) VALUES (@username, @name, @email, @password); SELECT LAST_INSERT_ID();",
                       DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@username", signupRequestModel.username);
                command.Parameters.AddWithValue("@name", signupRequestModel.name);
                command.Parameters.AddWithValue("@email", signupRequestModel.email);
                command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(signupRequestModel.password));

                var result = command.ExecuteScalar();
                userId = Convert.ToInt32(result); // Get the new user's ID
                
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "twoaxis.xyz",
                claims:[
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim("name", signupRequestModel.name),
                    new Claim(JwtRegisteredClaimNames.Email, signupRequestModel.email)
                ],
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds
            );

            DatabaseService.CloseConnection();
            
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new {
                token = tokenString
            });
        }
	[HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestModel loginRequestModel)
        {
            DatabaseService.OpenConnection();
            int userId = 0;
            string name = string.Empty;
            string email = string.Empty;
            string storedPasswordHash = string.Empty;
            using (var command = new MySqlCommand("SELECT id , name , email , password FROM users WHERE email = @email AND password = @password", DatabaseService.Connection))
            {
                command.Parameters.AddWithValue("@email", loginRequestModel.email);
                command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(loginRequestModel.password));
                var result = command.ExecuteReader();
                if (result.Read())
                    {
                        userId = result.GetInt32(0);
                        name = result.GetString(1);
                        email = result.GetString(2);
                        storedPasswordHash = result.GetString(3);
                    }
            }

            if ((loginRequestModel.email==email) && (HashUtil.GenerateSHA256Hash(loginRequestModel.password)==storedPasswordHash))
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "twoaxis.xyz",
                    claims:[
                        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                        new Claim("name", name),
                        new Claim(JwtRegisteredClaimNames.Email, email)
                    ],
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: creds
                );

                DatabaseService.CloseConnection();

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Ok(new {
                    token = tokenString
                });
                
            }
            else {
                DatabaseService.CloseConnection();
                return Unauthorized();
            }

        }
    }
    
}
