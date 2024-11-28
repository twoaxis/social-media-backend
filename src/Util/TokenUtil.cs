using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using social_media_backend.Exceptions.Auth;
using static System.Int32;

namespace social_media_backend.Util;

public static class TokenUtil
{
	private static string _secret = "";

	public static void Initialize(string secret)
	{
		_secret = secret;
	}

	public static string CreateToken(string id, string email)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: "twoaxis.xyz",
			claims:[
				new Claim(JwtRegisteredClaimNames.Sub, id),
				new Claim(JwtRegisteredClaimNames.Email, email)
			],
			expires: DateTime.Now.AddHours(24),
			signingCredentials: creds
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
	
	public static int ValidateToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.UTF8.GetBytes(_secret);

		var validationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = "twoaxis.xyz",
			ValidateAudience = false, // Set to true if you add an audience
			ValidateLifetime = true, // Checks for expiration
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(key)
		};
		try
		{

			var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

			if (validatedToken is not JwtSecurityToken jwtToken ||
			    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
				throw new SecurityTokenException("Invalid token");
			
			TryParse(principal.Claims.ToArray()[0].Value, out var result);
			return result;

		}
		catch (Exception ex)
		{
			throw new InvalidTokenException();
		}
	}
}
