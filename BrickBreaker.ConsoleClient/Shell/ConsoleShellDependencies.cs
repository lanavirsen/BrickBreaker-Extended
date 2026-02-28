using BrickBreaker.ConsoleClient.Game;
using BrickBreaker.ConsoleClient.Ui;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using BrickBreaker.Core.Clients;

namespace BrickBreaker.ConsoleClient.Shell;

public sealed class ConsoleShellDependencies
{
    public required ILoginMenu LoginMenu { get; init; }
    public required IGameplayMenu GameplayMenu { get; init; }
    public required IConsoleDialogs Dialogs { get; init; }
    public required IConsoleRenderer Renderer { get; init; }
    public required IGameApiClient ApiClient { get; init; }
    public required IGameHost GameHost { get; init; }
    public bool OwnsApiClient { get; init; }

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
            OwnsApiClient = true
        };
    }
}
