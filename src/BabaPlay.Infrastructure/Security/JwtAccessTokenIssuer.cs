using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BabaPlay.SharedKernel.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BabaPlay.Infrastructure.Security;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly JwtSettings _settings;

    public JwtAccessTokenIssuer(IOptions<JwtSettings> settings) => _settings = settings.Value;

    public string Issue(IReadOnlyCollection<Claim> claims)
    {
        var list = claims.ToList();
        if (!list.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
            list.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            list,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
