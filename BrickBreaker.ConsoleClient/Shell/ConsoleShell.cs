using System.Linq;
using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;
using System.Runtime.InteropServices;

namespace BrickBreaker.ConsoleClient.Shell;

public sealed class ConsoleShell : IDisposable
{
    private readonly ILoginMenu _loginMenu;
    private readonly IGameplayMenu _gameplayMenu;
    private readonly IConsoleDialogs _dialogs;
    private readonly IConsoleRenderer _renderer;
    private readonly IGameApiClient _apiClient;
    private readonly IGameHost _gameHost;
    private readonly bool _ownsApiClient;

    private string? _currentUser;
    private bool _isLoggedIn;
    private GameMode _currentMode = GameMode.Normal;

    public ConsoleShell(ConsoleShellDependencies? dependencies = null)
    {
        dependencies ??= ConsoleShellDependencies.CreateDefault();

        _loginMenu = dependencies.LoginMenu;
        _gameplayMenu = dependencies.GameplayMenu;
        _dialogs = dependencies.Dialogs;
        _renderer = dependencies.Renderer;
        _apiClient = dependencies.ApiClient;
        _gameHost = dependencies.GameHost;
        _ownsApiClient = dependencies.OwnsApiClient;
    }

    public async Task RunAsync()
    {
        AppState state = AppState.LoginMenu;
        while (state != AppState.Exit)
        {
            state = state switch
            {
                AppState.LoginMenu => await HandleLoginMenuAsync(),
                AppState.GameplayMenu => await HandleGameplayMenuAsync(),
                AppState.Playing => await HandlePlayingAsync(),
                _ => AppState.Exit
            };
        }
    }

    private async Task<AppState> HandleLoginMenuAsync()
    {
        var choice = _loginMenu.Show();
        return choice switch
        {
            LoginMenuChoice.QuickPlay => SetQuickPlayAndStart(),
            LoginMenuChoice.Register => await HandleRegisterAsync(),
            LoginMenuChoice.Login => await DoLoginAsync() ? AppState.GameplayMenu : AppState.LoginMenu,
            LoginMenuChoice.Leaderboard => await HandleLeaderboardAsync(),
            LoginMenuChoice.Exit => AppState.Exit,
            _ => AppState.LoginMenu
        };
    }

    private AppState SetQuickPlayAndStart()
    {
        ClearInputBuffer();
        _currentMode = GameMode.QuickPlay;
        return AppState.Playing;
    }

    private async Task<AppState> HandleRegisterAsync()
    {
        await DoRegisterAsync();
        _dialogs.Pause();
        return AppState.LoginMenu;
    }

    private async Task<AppState> HandleLeaderboardAsync()
    {
        await ShowLeaderboardAsync();
        _dialogs.Pause();
        return AppState.LoginMenu;
    }

    private async Task<AppState> HandleGameplayMenuAsync()
    {
        var choice = _gameplayMenu.Show(_currentUser ?? "guest");
        return choice switch
        {
            GameplayMenuChoice.Start => StartNormalPlay(),
            GameplayMenuChoice.Best => await ShowBestScoreAsyncAndStay(),
            GameplayMenuChoice.Leaderboard => await DisplayLeaderboardAndStayAsync(),
            GameplayMenuChoice.Logout => Logout(),
            _ => AppState.Exit
        };
    }

    private AppState StartNormalPlay()
    {
        _currentMode = GameMode.Normal;
        return AppState.Playing;
    }

    private async Task<AppState> ShowBestScoreAsyncAndStay()
    {
        await ShowBestScoreAsync();
        return AppState.GameplayMenu;
    }

    private async Task<AppState> DisplayLeaderboardAndStayAsync()
    {
        await ShowLeaderboardAsync();
        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    private AppState Logout()
    {
        _currentUser = null;
        _isLoggedIn = false;
        return AppState.LoginMenu;
    }

    private async Task<AppState> HandlePlayingAsync()
    {
        _renderer.ClearScreen();
        var score = _gameHost.Run();

        int lowerLine = Math.Max(GetWindowHeightSafe() - 4, 0);

        TrySetCursorPosition(0, lowerLine);
        _dialogs.ShowMessage($"\nFinal score: {score}");

        if (_currentMode != GameMode.QuickPlay && _isLoggedIn && score > 0)
        {
            await _renderer.RunStatusAsync("Submitting score...", () => _apiClient.SubmitScoreAsync(_currentUser!, score));
        }

        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    private async Task DoRegisterAsync()
    {
        if (!EnsureApiConfigured())
        {
            return;
        }

        var username = _dialogs.PromptNewUsername();
        var password = _dialogs.PromptNewPassword();

        var result = await _renderer.RunStatusAsync("Registering account...", () => _apiClient.RegisterAsync(username, password));
        _dialogs.ShowMessage(result.Success
            ? "Registration successful!"
            : result.Error ?? "Registration failed.");
    }

    private async Task<bool> DoLoginAsync()
    {
        if (!EnsureApiConfigured())
        {
            return false;
        }

        var (username, password) = _dialogs.PromptCredentials();

        var result = await _renderer.RunStatusAsync("Signing in...", () => _apiClient.LoginAsync(username, password));
        if (result.Success)
        {
            _currentUser = username;
            _isLoggedIn = true;
            return true;
        }

        _dialogs.ShowMessage(result.Error ?? "Login failed (wrong username or password).");
        return false;
    }

    private async Task ShowLeaderboardAsync()
    {
        _renderer.ClearScreen();
        _renderer.RenderHeader();

        if (!EnsureApiConfigured())
        {
            return;
        }

        var result = await _renderer.RunStatusAsync("Loading leaderboard...", () => _apiClient.GetLeaderboardAsync(10));
        if (!result.Success || result.Value is null)
        {
            ShowDatabaseWarning(result.Error ?? "Failed to load leaderboard.");
            return;
        }

        if (result.Value.Count == 0)
        {
            _dialogs.ShowMessage("\nTop 10 leaderboard:\nNo scores yet.");
            return;
        }

        var items = result.Value.Select(ToTuple);
        _dialogs.ShowLeaderboard(items);
    }

    private async Task ShowBestScoreAsync()
    {
        if (!EnsureApiConfigured() || !_isLoggedIn || string.IsNullOrWhiteSpace(_currentUser))
        {
            ShowDatabaseWarning("Best score lookup requires an active API connection and login.");
            return;
        }

        var result = await _renderer.RunStatusAsync("Fetching best score...", () => _apiClient.GetBestAsync(_currentUser!));
        if (!result.Success)
        {
            ShowDatabaseWarning(result.Error ?? "Failed to load best score.");
            return;
        }

        if (result.Value is null)
        {
            _dialogs.ShowMessage("\nNo scores recorded yet.");
        }
        else
        {
            var best = result.Value;
            _dialogs.ShowMessage($"\nYour best score: {best.Score} on {best.At.ToLocalTime():yyyy-MM-dd HH:mm}");
        }

        _dialogs.Pause();
    }

    private bool EnsureApiConfigured()
    {
        if (!string.IsNullOrWhiteSpace(_apiClient.BaseAddress))
        {
            return true;
        }

        ShowDatabaseWarning("API base URL not configured.");
        return false;
    }

    private static (string Username, int Score, DateTimeOffset At) ToTuple(ScoreEntry entry)
        => (entry.Username, entry.Score, entry.At);

    private static void ClearInputBuffer()
    {
        if (Console.IsInputRedirected)
        {
            return;
        }

        try
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }
        }
        catch
        {
            return;
        }

        try
        {
            var handle = GetStdHandle(-10);
            if (handle != IntPtr.Zero)
            {
                FlushConsoleInputBuffer(handle);
            }
        }
        catch
        {
        }
    }

    private static int GetWindowHeightSafe()
    {
        if (Console.IsOutputRedirected)
        {
            return 0;
        }

        try
        {
            return Console.WindowHeight;
        }
        catch
        {
            return 0;
        }
    }

    private static void TrySetCursorPosition(int left, int top)
    {
        if (Console.IsOutputRedirected)
        {
            return;
        }

        try
        {
            Console.SetCursorPosition(left, top);
        }
        catch
        {
        }
    }

    private void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }

    public void Dispose()
    {
        if (_ownsApiClient)
        {
            _apiClient.Dispose();
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);
}
