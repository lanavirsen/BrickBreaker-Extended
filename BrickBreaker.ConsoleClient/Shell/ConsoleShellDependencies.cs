using BrickBreaker.ConsoleClient.Game;
using BrickBreaker.ConsoleClient.Ui;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using BrickBreaker.Core.Clients;

namespace BrickBreaker.ConsoleClient.Shell;

// Dependency bundle passed to ConsoleShell. Grouping all dependencies into one
// object keeps the ConsoleShell constructor simple and makes it easy to swap
// individual services in tests without touching production wiring.
public sealed class ConsoleShellDependencies
{
    public required ILoginMenu LoginMenu { get; init; }       // Pre-login menu (quick play / login / register / leaderboard)
    public required IGameplayMenu GameplayMenu { get; init; } // Post-login menu (start / best score / leaderboard / logout)
    public required IConsoleDialogs Dialogs { get; init; }    // Prompts, messages, and pause screens
    public required IConsoleRenderer Renderer { get; init; }  // Header rendering and status spinners
    public required IGameApiClient ApiClient { get; init; }   // Auth and leaderboard HTTP calls
    public required IGameHost GameHost { get; init; }         // Runs a gameplay session and returns the final score

    // When true, ConsoleShell.Dispose() will dispose the ApiClient. Set to false
    // when the caller owns the client's lifetime (e.g. in tests).
    public bool OwnsApiClient { get; init; }

    // Builds the standard production dependency set. An optional base URL can be
    // passed from the command line; if omitted, ApiConfiguration resolves it from
    // environment variables or the bundled appsettings.
    public static ConsoleShellDependencies CreateDefault(string? preferredBase = null)
    {
        var apiBase = ApiConfiguration.ResolveBaseAddress(preferredBase);
        var apiClient = new GameApiClient(apiBase, turnstileBypassToken: ApiConfiguration.ResolveBypassToken());

        return new ConsoleShellDependencies
        {
            LoginMenu = new LoginMenu(),
            GameplayMenu = new GameplayMenu(),
            Dialogs = new ConsoleDialogs(),
            Renderer = new SpectreConsoleRenderer(new Header()),
            ApiClient = apiClient,
            GameHost = new BrickBreakerGame(),
            OwnsApiClient = true // this method created the client, so the shell must dispose it
        };
    }
}
