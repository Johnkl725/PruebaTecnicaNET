using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application;
using Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider clock) : ITokenGenerator
{
    public TokenDto Generate(User user)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow().UtcDateTime;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(user.Roles.SelectMany(role => role.Permissions)
            .Select(permission => permission.Name).Distinct(StringComparer.Ordinal)
            .Select(permission => new Claim("permission", permission)));
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now,
            now.AddMinutes(settings.DurationMinutes), credentials);
        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), "Bearer", settings.DurationMinutes * 60);
    }
}
