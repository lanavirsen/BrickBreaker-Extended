namespace BrickBreaker.Core.Abstractions;

/// <summary>
/// Evaluates text and reports whether it contains profanity or slurs.
/// </summary>
public interface IProfanityFilter
{
    bool ContainsProfanity(string? text);
}
