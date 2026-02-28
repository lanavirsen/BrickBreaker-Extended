namespace BrickBreaker.Api;

public sealed class TurnstileOptions
{
    public bool Enabled { get; set; }
    public string? SecretKey { get; set; }
    public string? BypassToken { get; set; }
    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(SecretKey);
}
