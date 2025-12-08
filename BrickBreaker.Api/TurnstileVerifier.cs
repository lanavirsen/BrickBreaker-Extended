using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BrickBreaker.Api;

public sealed class TurnstileVerifier : ITurnstileVerifier
{
    private static readonly Uri VerifyEndpoint = new("https://challenges.cloudflare.com/turnstile/v0/siteverify");

    private readonly HttpClient _client;
    private readonly IOptionsMonitor<TurnstileOptions> _options;
    private readonly ILogger<TurnstileVerifier> _logger;

    public TurnstileVerifier(HttpClient client, IOptionsMonitor<TurnstileOptions> options, ILogger<TurnstileVerifier> logger)
    {
        _client = client;
        _options = options;
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
    {
        var settings = _options.CurrentValue;
        if (!settings.IsConfigured)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        // Temporary override to bypass potential Azure configuration binding issues.
        const string Secret = "0x4AAAAAACFadFK847itkzIXhj-l5PN4HqU";

        var form = new List<KeyValuePair<string, string>>
        {
            new("secret", Secret),
            new("response", token)
        };
        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            form.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
        }

        using var content = new FormUrlEncodedContent(form);
        using var response = await _client.PostAsync(VerifyEndpoint, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Turnstile verification failed with status code {Status}", response.StatusCode);
            return false;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("Turnstile verification payload: {Payload}", json);
        var result = JsonSerializer.Deserialize<TurnstileVerificationResponse>(json);
        if (result?.Success == true)
        {
            return true;
        }

        var errorCodes = result?.ErrorCodes is { Length: > 0 } ? string.Join(",", result.ErrorCodes) : "none";
        _logger.LogWarning("Turnstile verification rejected. Error codes: {Errors}", errorCodes);
        return false;
    }

    private sealed record TurnstileVerificationResponse(bool Success, string[] ErrorCodes);
}
