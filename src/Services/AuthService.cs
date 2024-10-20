using MySql.Data.MySqlClient;
using social_media_backend.Exceptions;
using social_media_backend.Util;

namespace social_media_backend.Services;

public class AuthService
{
	public string Signup(string username, string name, string email, string password)
	{
		DatabaseService.OpenConnection();

		if (DoesUserExist(username, email)) throw new UserExistsException();

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

		DatabaseService.CloseConnection();

		return TokenUtil.CreateToken(userId.ToString(), email);
	}

	public string Login(string email, string password)
	{
		DatabaseService.OpenConnection();
		
		if(!DoesUserExist(email)) throw new InvalidCredentialsException();
		
		DatabaseService.OpenConnection();
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
		DatabaseService.CloseConnection();

		if (HashUtil.GenerateSHA256Hash(password) != storedPasswordHash) throw new InvalidCredentialsException();
			
		return TokenUtil.CreateToken(userId.ToString(), email);
	}
	private static bool DoesUserExist(string username, string email)
	{
		using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username OR email = @email", DatabaseService.Connection);
		command.Parameters.AddWithValue("@username", username);
		command.Parameters.AddWithValue("@email", email);
		var result = command.ExecuteScalar();
		return Convert.ToInt32(result) > 0;
	}
	private static bool DoesUserExist(string email)
	{
		using var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", DatabaseService.Connection);
		command.Parameters.AddWithValue("@email", email);
		var result = command.ExecuteScalar();
		return Convert.ToInt32(result) > 0;
	}
}