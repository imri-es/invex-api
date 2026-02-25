using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using invex_api.Models;
using Microsoft.IdentityModel.Tokens;

namespace invex_api.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _config = config;
        var jwtKey =
            _config["JWT_KEY"]
            ?? throw new InvalidOperationException("JWT_KEY is missing from configuration");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    }

    public string CreateToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
            new Claim("Role", user.AccessRole.ToString()),
            // Add other claims as needed
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["JWT_ISSUER"],
            Audience = _config["JWT_AUDIENCE"],
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
