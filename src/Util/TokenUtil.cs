using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
}