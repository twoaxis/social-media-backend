using MySql.Data.MySqlClient;
using social_media_backend.Exceptions;
using social_media_backend.Exceptions.Auth;
using social_media_backend.src.Services;
using social_media_backend.Util;
using System.IdentityModel.Tokens.Jwt;
using social_media_backend.src.Exceptions;

namespace social_media_backend.Services;

public class AuthService
{
	private readonly UserService _userService = new();
	private readonly CodeService _codeService = new();
	
	public int Signup(string username, string name, string email, string password)
	{
		try
		{
			DatabaseService.OpenConnection();
			if (_userService.DoesUserExistByEmail(email)) throw new EmailTakenException();
			if (_userService.DoesUserExistByUsername(username)) throw new UserExistsException();
			
			using var command = new MySqlCommand("INSERT INTO users (username, name, email, password) VALUES (@username, @name, @email, @password); SELECT LAST_INSERT_ID();",
				DatabaseService.Connection);
			
			command.Parameters.AddWithValue("@username", username);
			command.Parameters.AddWithValue("@name", name);
			command.Parameters.AddWithValue("@email", email);
			command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(password));

			var result = command.ExecuteScalar();
			var userId = Convert.ToInt32(result); // Get the new user's ID

			return userId;
		}
		finally
		{
			DatabaseService.CloseConnection();
		}
	}

	public void UpdatePassword(int uid, string password)
	{

		try
		{
			DatabaseService.OpenConnection();

			using var command = new MySqlCommand("UPDATE users SET password = @password WHERE id = @uid",
				DatabaseService.Connection);
			command.Parameters.AddWithValue("@id", uid);
			command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(password));

			command.ExecuteNonQuery();
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
			var username = string.Empty;

			using var command = new MySqlCommand("SELECT id, username, email, password, email_verified FROM users WHERE email = @email AND password = @password", DatabaseService.Connection);
			command.Parameters.AddWithValue("@email", email);
			command.Parameters.AddWithValue("@password", HashUtil.GenerateSHA256Hash(password));
			var result = command.ExecuteReader();
			
			if (result.Read())
			{
				userId = result.GetInt32("id");
				storedPasswordHash = result.GetString("password");
				username = result.GetString("username");
				if (!result.GetBoolean("email_verified")) throw new UserNotVerifiedException(userId);
			}

			if (HashUtil.GenerateSHA256Hash(password) != storedPasswordHash) throw new InvalidCredentialsException();
			
			return TokenUtil.CreateToken(userId.ToString(), email, username);
		}
		finally
		{
			DatabaseService.CloseConnection();
		}

	}

	public string VerifyMail(int uid)
	{
		DatabaseService.OpenConnection();
		
		try
		{
			using (var updateCommand = new MySqlCommand("UPDATE users SET email_verified=TRUE WHERE id = @id", DatabaseService.Connection))
			{
				updateCommand.Parameters.AddWithValue("@id", uid);
				updateCommand.ExecuteNonQuery();
			}

			var email = String.Empty;
			var username = String.Empty;
			using (var selectCommand = new MySqlCommand("SELECT email, username FROM users WHERE id = @id", DatabaseService.Connection))
			{
				selectCommand.Parameters.AddWithValue("@id", uid);
				var result = selectCommand.ExecuteReader();

				if (result.Read())
				{
					email = result.GetString("email");
					username = result.GetString("username");
				}
			}
			
			return TokenUtil.CreateToken(uid.ToString(), email, username);
		}
		finally
		{
			DatabaseService.CloseConnection();
		}
	}

	
	public void Logout(string token){
		DatabaseService.OpenConnection();
		try
		{
			using var command = new MySqlCommand("INSERT INTO revoked_tokens (token) VALUES (@token)",DatabaseService.Connection);
			
			command.Parameters.AddWithValue("@token", token);
			command.ExecuteNonQuery();
		}
		finally
		{
			DatabaseService.CloseConnection();
		}

	}
	
}