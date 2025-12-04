using System;

namespace BrickBreaker.Game;

/// <summary>
/// Placeholder used when the game project is built on a non-Windows platform.
/// The real game engine relies on WinForms and System.Drawing, which are only supported on Windows.
/// </summary>
internal static class GameUnavailable
{
    /// <summary>
    /// Throws a clear exception so any accidental runtime use fails fast with a helpful message.
    /// </summary>
    public static void Throw()
        => throw new PlatformNotSupportedException(
            "BrickBreaker.Game is only available when building on Windows. " +
            "Switch to the net9.0-windows7.0 target to access the real game engine.");
}
