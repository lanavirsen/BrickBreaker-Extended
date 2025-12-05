using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using Spectre.Console;

namespace BrickBreaker.ConsoleClient.Shell;

internal sealed class SpectreConsoleRenderer : IConsoleRenderer
{
    private readonly Header _header;

    public SpectreConsoleRenderer(Header header)
    {
        _header = header;
    }

    public void ClearScreen() => AnsiConsole.Clear();

    public void RenderHeader() => _header.TitleHeader();

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
