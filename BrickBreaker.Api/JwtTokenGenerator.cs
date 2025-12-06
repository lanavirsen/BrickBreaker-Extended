using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BrickBreaker.Api;

public interface IJwtTokenGenerator
{
    string GenerateToken(string username);
}

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _credentials;

    public JwtTokenGenerator(JwtOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(_options.Secret))
        {
            throw new InvalidOperationException("JWT secret must be configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public string GenerateToken(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required to generate a token.", nameof(username));
        }

        var normalized = username.Trim();
        var handler = new JwtSecurityTokenHandler();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, normalized),
            new Claim(JwtRegisteredClaimNames.UniqueName, normalized),
            new Claim(ClaimTypes.Name, normalized),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Math.Max(1, _options.ExpirationMinutes)),
            signingCredentials: _credentials);

        return handler.WriteToken(token);
    }
}
