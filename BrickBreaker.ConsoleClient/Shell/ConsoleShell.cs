using BrickBreaker.ConsoleClient.Game;
using BrickBreaker.ConsoleClient.Ui;
using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;
using Spectre.Console;
using System.Linq;
using System.Runtime.InteropServices;

namespace BrickBreaker.ConsoleClient.Shell;

internal sealed class ConsoleShell : IDisposable
{
    private readonly ILoginMenu _loginMenu = new LoginMenu();
    private readonly IGameplayMenu _gameplayMenu = new GameplayMenu();
    private readonly IConsoleDialogs _dialogs = new ConsoleDialogs();
    private readonly Header _header = new();
    private readonly GameApiClient _apiClient = new();

    private string? _currentUser;
    private bool _isLoggedIn;
    private GameMode _currentMode = GameMode.Normal;
    private bool _apiConfigured;

    public ConsoleShell()
    {
        try
        {
            var apiBase = ApiConfiguration.ResolveBaseAddress();
            _apiClient.SetBaseAddress(apiBase);
            _apiConfigured = true;
        }
        catch (Exception ex)
        {
            ShowDatabaseWarning($"Failed to initialize API client: {ex.Message}");
        }
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
        AnsiConsole.Clear();
        IGame game = new BrickBreakerGame();
        int score = game.Run();

        int lowerLine = Console.WindowHeight - 4;
        if (lowerLine < 0)
        {
            lowerLine = 0;
        }

        Console.SetCursorPosition(0, lowerLine);
        _dialogs.ShowMessage($"\nFinal score: {score}");

        if (_currentMode != GameMode.QuickPlay && _isLoggedIn && _apiConfigured && score > 0)
        {
            await RunWithSpinnerAsync("Submitting score...", () => _apiClient.SubmitScoreAsync(_currentUser!, score));
        }

        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    private async Task DoRegisterAsync()
    {
        if (!_apiConfigured)
        {
            ShowDatabaseWarning("API base URL not configured.");
            return;
        }

        var username = _dialogs.PromptNewUsername();
        var password = _dialogs.PromptNewPassword();

        var result = await RunWithSpinnerAsync("Registering account...", () => _apiClient.RegisterAsync(username, password));
        _dialogs.ShowMessage(result.Success
            ? "Registration successful!"
            : result.Error ?? "Registration failed.");
    }

    private async Task<bool> DoLoginAsync()
    {
        if (!_apiConfigured)
        {
            ShowDatabaseWarning("API base URL not configured.");
            return false;
        }

        var (username, password) = _dialogs.PromptCredentials();

        var result = await RunWithSpinnerAsync("Signing in...", () => _apiClient.LoginAsync(username, password));
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
        AnsiConsole.Clear();
        _header.TitleHeader();

        if (!_apiConfigured)
        {
            ShowDatabaseWarning("Leaderboard is unavailable because the API base URL is not configured.");
            return;
        }

        var result = await RunWithSpinnerAsync("Loading leaderboard...", () => _apiClient.GetLeaderboardAsync(10));
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
        if (!_apiConfigured || !_isLoggedIn || string.IsNullOrWhiteSpace(_currentUser))
        {
            ShowDatabaseWarning("Best score lookup requires an active API connection and login.");
            return;
        }

        var result = await RunWithSpinnerAsync("Fetching best score...", () => _apiClient.GetBestAsync(_currentUser!));
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

    private static (string Username, int Score, DateTimeOffset At) ToTuple(ScoreEntry entry)
        => (entry.Username, entry.Score, entry.At);

    private static void ClearInputBuffer()
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
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

    private void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }

    private async Task<T> RunWithSpinnerAsync<T>(string description, Func<Task<T>> action)
    {
        T result = default!;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(description, async _ => result = await action());
        return result;
    }

    public void Dispose()
    {
        _apiClient.Dispose();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);
}
