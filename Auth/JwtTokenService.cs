using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Masarif.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Masarif.Api.Auth;

public interface IJwtTokenService
{
    (string token, DateTime expiresUtc) CreateAccessToken(AppUser user);
}

public sealed class JwtTokenService(JwtOptions options) : IJwtTokenService
{
    private readonly JwtOptions _opt = options;

    public (string token, DateTime expiresUtc) CreateAccessToken(AppUser user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_opt.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("ver", user.TokenVersion.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
