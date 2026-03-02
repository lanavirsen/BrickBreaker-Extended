namespace BrickBreaker.ConsoleClient.Shell;

// Abstracts the shell-level rendering concerns: clearing the screen, drawing the
// app header, and running async operations behind a status spinner. Keeping this
// behind an interface lets tests inject a no-op implementation without pulling in
// Spectre.Console.
public interface IConsoleRenderer
{
    // Clears all console output, ready for a fresh screen.
    void ClearScreen();

    // Renders the application title header above menu content.
    void RenderHeader();

    // Runs an async operation while showing a spinner labelled with description.
    // Returns whatever the operation produces, so callers can chain result handling
    // without needing a separate awaitable and result variable.
    Task<T> RunStatusAsync<T>(string description, Func<Task<T>> action);
}
