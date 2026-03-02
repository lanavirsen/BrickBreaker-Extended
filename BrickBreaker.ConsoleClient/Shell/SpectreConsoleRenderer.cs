using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using Spectre.Console;

namespace BrickBreaker.ConsoleClient.Shell;

// Spectre.Console implementation of IConsoleRenderer. Handles ANSI screen
// clearing, header rendering, and animated status spinners for async operations.
internal sealed class SpectreConsoleRenderer : IConsoleRenderer
{
    private readonly Header _header;

    public SpectreConsoleRenderer(Header header)
    {
        _header = header;
    }

    // Clears the terminal using Spectre's ANSI-aware clear so any colour or
    // style state is also reset.
    public void ClearScreen() => AnsiConsole.Clear();

    // Delegates header rendering to the shared Header component so the title
    // style stays consistent across all screens.
    public void RenderHeader() => _header.TitleHeader();

    // Wraps an async operation in a Spectre status spinner. The result is
    // captured via closure because Spectre's StartAsync callback does not support
    // a return value directly.
    public async Task<T> RunStatusAsync<T>(string description, Func<Task<T>> action)
    {
        T result = default!;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(description, async _ => result = await action());
        return result;
    }
}
