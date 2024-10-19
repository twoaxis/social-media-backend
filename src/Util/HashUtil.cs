using System.Security.Cryptography;
using System.Text;

namespace social_media_backend.Util;

public class HashUtil
{
	public static string GenerateSHA256Hash(string password)
	{
		using (SHA256 sha256Hash = SHA256.Create())
		{
			byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
			StringBuilder builder = new StringBuilder();
			foreach (byte b in bytes)
			{
				builder.Append(b.ToString("x2"));
			}

			return builder.ToString();
		}
	}
}