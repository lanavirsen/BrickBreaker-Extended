using System;
using BrickBreaker.Game;

namespace BrickBreaker.Hosting;

/// <summary>
/// Minimal placeholder used when the WinForms host is built on non-Windows platforms.
/// It ensures the solution continues to compile (so test projects can run) while
/// making it clear that the actual UI is unavailable.
/// </summary>
public sealed class WinFormsBrickBreakerGame : IGame
{
    public int Run()
    {
        throw new PlatformNotSupportedException(
            "The WinForms version of BrickBreaker can only run on Windows. " +
            "Build the project for the net9.0-windows7.0 target to play the game.");
    }
}
