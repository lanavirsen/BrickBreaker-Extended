namespace BrickBreaker.Api;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = "BrickBreaker.Api";
    public string Audience { get; set; } = "BrickBreaker.Clients";
    public string Secret { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
