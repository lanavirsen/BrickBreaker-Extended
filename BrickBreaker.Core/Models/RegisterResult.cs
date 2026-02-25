namespace BrickBreaker.Core.Models;

/// <summary>
/// Represents the result of a registration attempt with detailed error information.
/// </summary>
/// <param name="Success">Indicates whether registration succeeded.</param>
/// <param name="ErrorCode">Optional error code when registration fails (e.g., "password_too_short", "username_taken").</param>
public sealed record RegisterResult(bool Success, string? ErrorCode = null)
{
    /// <summary>
    /// Creates a successful registration result.
    /// </summary>
    public static RegisterResult Ok() => new(true);

    /// <summary>
    /// Creates a failed registration result with an error code.
    /// </summary>
    public static RegisterResult Fail(string errorCode) => new(false, errorCode);
}
