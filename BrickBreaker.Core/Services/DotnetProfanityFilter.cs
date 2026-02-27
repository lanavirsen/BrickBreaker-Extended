using BrickBreaker.Core.Abstractions;
using DotnetBadWordDetector;

namespace BrickBreaker.Core.Services;

// Wraps the DotnetBadWordDetector to provide profanity evaluation.
public sealed class DotnetProfanityFilter : IProfanityFilter
{
    private readonly Lazy<ProfanityDetector> _detector = new(() => new ProfanityDetector());

    public bool ContainsProfanity(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return _detector.Value.IsProfane(text);
    }
}
