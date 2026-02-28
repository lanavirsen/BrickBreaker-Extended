using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BrickBreaker.Core.Clients;

/// <summary>
/// Provides shared helpers for configuring API clients across UI surfaces.
/// </summary>
public static class ApiConfiguration
{
    public const string DefaultBaseAddress = "https://brickbreaker-api.delightfulsky-8a169c96.swedencentral.azurecontainerapps.io/";
    public const string BaseAddressEnvironmentVariable = "BRICKBREAKER_API_URL";
    public const string BypassTokenEnvironmentVariable = "BRICKBREAKER_BYPASS_TOKEN";
    public const string SettingsFileEnvironmentVariable = "BRICKBREAKER_CLIENT_CONFIG";
    public const string DefaultSettingsFileName = "clientsettings.json";

    public static string ResolveBaseAddress(string? preferred = null, string? settingsPath = null)
    {
        var candidate = ChooseCandidate(preferred, settingsPath);
        return NormalizeBaseAddress(candidate ?? DefaultBaseAddress);
    }

    /// <summary>
    /// Resolves the Turnstile bypass token for desktop clients that cannot show a CAPTCHA widget.
    /// Resolution order: env var BRICKBREAKER_BYPASS_TOKEN → clientsettings.json TurnstileBypassToken → null.
    /// Returns null when no token is configured, which means no bypass is attempted.
    /// </summary>
    public static string? ResolveBypassToken(string? settingsPath = null)
    {
        var envValue = TryGetEnvironmentVariable(BypassTokenEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue;
        }

        return LoadStringFromSettings("TurnstileBypassToken", settingsPath);
    }

    private static string? ChooseCandidate(string? preferred, string? settingsPath)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            return preferred;
        }

        var envValue = TryGetEnvironmentVariable(BaseAddressEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue;
        }

        var configOverride = settingsPath ?? TryGetEnvironmentVariable(SettingsFileEnvironmentVariable);
        return LoadFromSettings(configOverride);
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

    private static string? LoadFromSettings(string? overridePath)
        => LoadStringFromSettings("ApiBaseUrl", overridePath);

    private static string? LoadStringFromSettings(string propertyName, string? overridePath)
    {
        if (OperatingSystem.IsBrowser())
        {
            return null;
        }

        foreach (var path in EnumerateCandidatePaths(overridePath))
        {
            try
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                using var stream = File.OpenRead(path);
                using var document = JsonDocument.Parse(stream);

                if (document.RootElement.TryGetProperty(propertyName, out var property))
                {
                    var value = property.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            catch
            {
                // Ignore malformed config files and proceed to the next candidate.
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateCandidatePaths(string? overridePath)
    {
        if (OperatingSystem.IsBrowser())
        {
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(overridePath))
        {
            var normalized = Path.GetFullPath(overridePath);
            if (seen.Add(normalized))
            {
                yield return normalized;
            }
        }

        foreach (var dir in EnumerateProbeDirectories())
        {
            var candidate = Path.Combine(dir, DefaultSettingsFileName);
            candidate = Path.GetFullPath(candidate);

            if (seen.Add(candidate))
            {
                yield return candidate;
            }
        }
    }

    private static IEnumerable<string> EnumerateProbeDirectories()
    {
        if (OperatingSystem.IsBrowser())
        {
            yield break;
        }

        yield return AppContext.BaseDirectory;
        yield return Directory.GetCurrentDirectory();

        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory?.Parent is not null)
        {
            directory = directory.Parent;
            yield return directory.FullName;
        }
    }

    private static string? TryGetEnvironmentVariable(string variable)
    {
        try
        {
            return Environment.GetEnvironmentVariable(variable);
        }
        catch (PlatformNotSupportedException)
        {
            return null;
        }
    }
}
