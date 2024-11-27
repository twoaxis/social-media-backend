using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using social_media_backend.Exceptions;

namespace social_media_backend.Services;

public class CodeService
{
    private string GenerateCode()
    {
        return new Random().Next(0, 1000000).ToString("D6");
    }

    private string GenerateSessionId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new StringBuilder(64);

        for (var i = 0; i < 64; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }
        
        return result.ToString();
    }

    public string CreateNewEmailVerificationCode(string email, int userId)
    {
        var code = GenerateCode();
        var sessionId = GenerateSessionId();
        
        DatabaseService.OpenConnection();

        try
        {
            using var command =
                new MySqlCommand("INSERT INTO email_verification_codes (uid, code, session_id) VALUES (@uid, @code, @sessionId)",
                    DatabaseService.Connection);
            
            command.Parameters.AddWithValue("@uid", userId);
            command.Parameters.AddWithValue("@code", code);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            
            command.ExecuteNonQuery();
            
            Console.WriteLine($"The verification code for {email} is {code}");
            
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                using var httpClient = new HttpClient();
                
                httpClient.DefaultRequestHeaders.Add("api-key", Environment.GetEnvironmentVariable("BREVO_API_KEY"));

                var jsonContent = $"{{" +
                                  $"\"subject\": \"Verify your TwoAxis Social Email\"," +
                                  $"\"htmlContent\": \"<html><body><p>{code}</p></body></html>\"," +
                                  $"\"sender\": {{ \"email\": \"noreply@twoaxis.xyz\", \"name\": \"TwoAxis Social\" }}," +
                                  $"\"to\": [{{ \"email\": \"{email}\" }}]" +
                                  $"}}";

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", content)
                    .GetAwaiter().GetResult();

                response.EnsureSuccessStatusCode();
            }

        }
        finally
        {
            DatabaseService.CloseConnection();
        }
        
        return sessionId;
    }

    public int VerifyCode(string sessionId, string code)
    {
        DatabaseService.OpenConnection();

        try
        {
            using var command = new MySqlCommand("SELECT uid FROM email_verification_codes WHERE session_id = @sessionId AND code = @code", DatabaseService.Connection);
            
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.Parameters.AddWithValue("@code", code);
            
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var uid = Convert.ToInt32(reader["uid"]);
                reader.Close();

                using var deleteCommand = new MySqlCommand("DELETE FROM email_verification_codes WHERE uid=@uid", DatabaseService.Connection);
                deleteCommand.Parameters.AddWithValue("@uid", uid);
                deleteCommand.ExecuteNonQuery();

                return uid;
            }
            else throw new InvalidVerificationCodeException(); 
        }
        finally
        {
            DatabaseService.CloseConnection();
        }
    }
}