using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public static class TokenHelper
{
    public static string GenerateToken(string subject, DateTime expires, string issuer, string audience, string key)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("sub", subject) }),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = issuer,
            Audience = audience
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }
}