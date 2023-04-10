using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Options;

public sealed class Role : IEquatable<Role>
{
    private Role(string value) { Value = value; }
    private string Value { get; }
    public static Role Admin { get { return new Role("Admin"); } }
    public static Role User { get { return new Role("User"); } }
    public static implicit operator string(Role r) => r.Value;

    public bool Equals(Role? other) => other != null && other == Value;

    public override bool Equals(object? obj) => Equals(obj as Role);

    public override int GetHashCode() => Value.GetHashCode();
}

public class TokenProvider
{
    private readonly Jwt jwtSettings;

    public TokenProvider(IOptions<AppSettings> appSettings)
    {
        jwtSettings = appSettings.Value.Jwt;
    }

    public string GenerateToken(Guid nameIdentifier, Role role, DateTime expires)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier.ToString()),
                new Claim(ClaimTypes.Role, role)
                }),
            Expires = expires,
            SigningCredentials = credentials,
            Issuer = jwtSettings.Issuer,
            Audience = jwtSettings.Audience,
            IssuedAt = DateTime.Now
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }
}