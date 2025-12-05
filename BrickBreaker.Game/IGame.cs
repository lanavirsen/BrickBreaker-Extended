namespace BrickBreaker.Game;

// Shared abstraction for running a playable BrickBreaker session.
// Implementations can provide their own presentation (console, WinForms, etc.)
// but must return the achieved score once the session ends.
public interface IGame
{
    // Starts the gameplay loop and blocks until the session ends.
    // The returned integer represents the player's final score.
    int Run();
}
