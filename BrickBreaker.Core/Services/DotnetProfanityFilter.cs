using BrickBreaker.Core.Abstractions;
using DotnetBadWordDetector;

namespace BrickBreaker.Core.Services;

/// <summary>
/// Wraps the DotnetBadWordDetector to provide profanity evaluation.
/// </summary>
public sealed class DotnetProfanityFilter : IProfanityFilter
{
    private readonly ProfanityDetector _detector;

    public DotnetProfanityFilter()
    {
        _detector = new ProfanityDetector();
    }

    public bool ContainsProfanity(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return _detector.IsProfane(text);
    }
}
