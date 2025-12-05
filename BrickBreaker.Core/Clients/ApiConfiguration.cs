using System;

namespace BrickBreaker.Core.Clients;

/// <summary>
/// Provides shared helpers for configuring API clients across UI surfaces.
/// </summary>
public static class ApiConfiguration
{
    public const string DefaultBaseAddress = "http://127.0.0.1:5080/";
    public const string BaseAddressEnvironmentVariable = "BRICKBREAKER_API_URL";

    public static string ResolveBaseAddress(string? preferred = null)
    {
        var candidate = !string.IsNullOrWhiteSpace(preferred)
            ? preferred
            : Environment.GetEnvironmentVariable(BaseAddressEnvironmentVariable);

        return NormalizeBaseAddress(candidate ?? DefaultBaseAddress);
    }

    public static string NormalizeBaseAddress(string baseAddress)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new ArgumentException("API base address cannot be empty.", nameof(baseAddress));
        }

        var trimmed = baseAddress.Trim();
        if (!trimmed.EndsWith("/", StringComparison.Ordinal))
        {
            trimmed += "/";
        }

        return trimmed;
    }
}
