namespace BrickBreaker.WebClient;

public sealed class TurnstileClientOptions
{
    public bool Enabled { get; set; }
    public string? SiteKey { get; set; }

    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(SiteKey);
}
