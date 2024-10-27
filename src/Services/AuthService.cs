using MySql.Data.MySqlClient;
using social_media_backend.Exceptions;
using social_media_backend.Exceptions.Auth;
using social_media_backend.src.Services;
using social_media_backend.Util;
using System.IdentityModel.Tokens.Jwt;
namespace social_media_backend.Services;

public class AuthService
{
	private readonly UserService _userService = new();
	
	public string Signup(string username, string name, string email, string password)
	{
		try
		{
			DatabaseService.OpenConnection();
			if (_userService.DoesUserExistByEmail(email)) throw new EmailTakenException();
			if (_userService.DoesUserExistByUsername(username)) throw new UserExistsException();
			
			int userId;
			using (var command = new MySqlCommand("INSERT INTO users (username, name, email, password) VALUES (@username, @name, @email, @password); SELECT LAST_INSERT_ID();",
				       DatabaseService.Connection))
			{
				command.Parameters.AddWithValue("@username", username);
				command.Parameters.AddWithValue("@name", name);
				command.Parameters.AddWithValue("@email", email);
				command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(password));

				var result = command.ExecuteScalar();
				userId = Convert.ToInt32(result); // Get the new user's ID
                
			}
			return TokenUtil.CreateToken(userId.ToString(), email);
		}
		finally
		{
			DatabaseService.CloseConnection();
		}
	}

	public string Login(string email, string password)
	{
		DatabaseService.OpenConnection();

		try
		{
			if(!_userService.DoesUserExistByEmail(email)) throw new InvalidCredentialsException();
		
			var userId = -1;
			var storedPasswordHash = string.Empty;
		
			using (var command = new MySqlCommand("SELECT id , email , password FROM users WHERE email = @email AND password = @password", DatabaseService.Connection))
			{
				command.Parameters.AddWithValue("@email", email);
				command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(password));
				var result = command.ExecuteReader();
				if (result.Read())
				{
					userId = result.GetInt32("id");
					storedPasswordHash = result.GetString("password");
				}
			}
			
			if (HashUtil.GenerateSHA256Hash(password) != storedPasswordHash) throw new InvalidCredentialsException();
			
			return TokenUtil.CreateToken(userId.ToString(), email);
		}
		finally
		{
			DatabaseService.CloseConnection();
		}

	}

	private int? GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

        if (jwtToken == null)
            return null;

        var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
    }

	private static bool DoesUserExistByID(int UserID)
	{
		using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE id = @UserID", DatabaseService.Connection);
		command.Parameters.AddWithValue("@UserID", UserID);
		var result = command.ExecuteScalar();
		return Convert.ToInt32(result) > 0;
	}
	public bool Logout(string token){
		var ID=GetUserIdFromToken(token);
		if(ID == null) return false;
		DatabaseService.OpenConnection();
		if(!DoesUserExistByID(ID.Value)){
			DatabaseService.CloseConnection();
			return false;
		}
		using(var command = new MySqlCommand("INSERT INTO revoked_tokens (token) VALUES (@token)",DatabaseService.Connection))
		{
			command.Parameters.AddWithValue("@token", token);
			command.ExecuteNonQuery();
		}
		DatabaseService.CloseConnection();
		return true;

	}
	
}