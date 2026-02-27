namespace BrickBreaker.Api;

// Development-only verifier that bypasses Turnstile to simplify local testing.
public sealed class DisabledTurnstileVerifier : ITurnstileVerifier
{
    private readonly ILogger<DisabledTurnstileVerifier> _logger;
    private bool _warned;

    public DisabledTurnstileVerifier(ILogger<DisabledTurnstileVerifier> logger)
    {
        _logger = logger;
    }

    public Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken cancellationToken)
    {
        if (!_warned)
        {
            _logger.LogWarning("Turnstile verification is disabled. CAPTCHA enforcement is bypassed for this environment.");
            _warned = true;
        }

        return Task.FromResult(true);
    }
}
