using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;
using System.Runtime.InteropServices;

namespace BrickBreaker.ConsoleClient.Shell;

// Top-level application controller. Owns the app-state machine and coordinates
// all user-facing flows: login, registration, leaderboard, and gameplay.
public sealed class ConsoleShell : IDisposable
{
    // UI and service dependencies injected at construction time.
    private readonly ILoginMenu _loginMenu;
    private readonly IGameplayMenu _gameplayMenu;
    private readonly IConsoleDialogs _dialogs;
    private readonly IConsoleRenderer _renderer;
    private readonly IGameApiClient _apiClient;
    private readonly IGameHost _gameHost;

    // True when this shell created the ApiClient itself and must dispose it.
    private readonly bool _ownsApiClient;

    // Session state — reset on logout or quick-play.
    private string? _currentUser;
    private bool _isLoggedIn;
    private GameMode _currentMode = GameMode.Normal;

    // Accepts an optional dependency bundle; falls back to production defaults.
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

    // Main entry point. Drives the app-state machine until the user exits.
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

    // Displays the pre-login menu and routes each choice to its handler.
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

    // Starts an unauthenticated session. Clears any stale credentials so a
    // previous login cannot accidentally carry over into a quick-play run.
    private AppState SetQuickPlayAndStart()
    {
        ClearInputBuffer();
        _currentMode = GameMode.QuickPlay;
        _apiClient.ClearAuthentication();
        _currentUser = null;
        _isLoggedIn = false;
        return AppState.Playing;
    }

    // Runs the registration flow and returns to the login menu afterwards.
    private async Task<AppState> HandleRegisterAsync()
    {
        await DoRegisterAsync();
        _dialogs.Pause();
        return AppState.LoginMenu;
    }

    // Shows the leaderboard from the login menu, then returns to it.
    private async Task<AppState> HandleLeaderboardAsync()
    {
        await ShowLeaderboardAsync();
        _dialogs.Pause();
        return AppState.LoginMenu;
    }

    // Displays the post-login gameplay menu and routes each choice.
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

    // Marks the session as a scored run and starts the game.
    private AppState StartNormalPlay()
    {
        _currentMode = GameMode.Normal;
        return AppState.Playing;
    }

    // Shows the player's personal best then returns to the gameplay menu.
    private async Task<AppState> ShowBestScoreAsyncAndStay()
    {
        await ShowBestScoreAsync();
        return AppState.GameplayMenu;
    }

    // Shows the global leaderboard then returns to the gameplay menu.
    private async Task<AppState> DisplayLeaderboardAndStayAsync()
    {
        await ShowLeaderboardAsync();
        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    // Clears all session state and drops back to the login menu.
    private AppState Logout()
    {
        _currentUser = null;
        _isLoggedIn = false;
        _apiClient.ClearAuthentication();
        return AppState.LoginMenu;
    }

    // Runs one gameplay session. After the game ends, shows the final score and
    // submits it to the leaderboard if the player is logged in on a normal run.
    private async Task<AppState> HandlePlayingAsync()
    {
        _renderer.ClearScreen();
        var score = _gameHost.Run();

        // Position the score message near the bottom of the visible window so it
        // doesn't overlap the game area that was just rendered.
        int lowerLine = Math.Max(GetWindowHeightSafe() - 4, 0);

        TrySetCursorPosition(0, lowerLine);
        _dialogs.ShowMessage($"\nFinal score: {score}");

        // Only submit when there is something worth recording: a logged-in normal
        // run with a non-zero score.
        if (_currentMode != GameMode.QuickPlay && _isLoggedIn && score > 0)
        {
            await _renderer.RunStatusAsync("Submitting score...", () => _apiClient.SubmitScoreAsync(_currentUser!, score));
        }

        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    // Prompts for credentials and calls the registration API.
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

    // Prompts for credentials, calls the login API, and updates session state on
    // success. Returns true if the user is now authenticated.
    private async Task<bool> DoLoginAsync()
    {
        if (!EnsureApiConfigured())
        {
            return false;
        }

        var (username, password) = _dialogs.PromptCredentials();

        var result = await _renderer.RunStatusAsync("Signing in...", () => _apiClient.LoginAsync(username, password));
        if (result.Success && result.Value is not null)
        {
            _currentUser = result.Value.Username;
            _isLoggedIn = true;
            return true;
        }

        _dialogs.ShowMessage(result.Error ?? "Login failed (wrong username or password).");
        return false;
    }

    // Fetches and renders the top-10 global leaderboard.
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

    // Fetches and displays the current user's personal best score.
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

    // Guard used before any API call. Shows an error and returns false when the
    // base URL is not configured (i.e. running fully offline).
    private bool EnsureApiConfigured()
    {
        if (!string.IsNullOrWhiteSpace(_apiClient.BaseAddress))
        {
            return true;
        }

        ShowDatabaseWarning("API base URL not configured.");
        return false;
    }

    // Projects a ScoreEntry domain object to the anonymous tuple the dialog layer expects.
    private static (string Username, int Score, DateTimeOffset At) ToTuple(ScoreEntry entry)
        => (entry.Username, entry.Score, entry.At);

    // Flushes any queued key events before starting a game so stale input cannot
    // immediately fire in-game actions. Two strategies are used in sequence:
    // the managed Console API first, then a Win32 flush for any events the managed
    // layer may have missed.
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
            var handle = GetStdHandle(-10); // STD_INPUT_HANDLE = -10
            if (handle != IntPtr.Zero)
            {
                FlushConsoleInputBuffer(handle);
            }
        }
        catch
        {
        }
    }

    // Returns the console window height, or 0 if the output is redirected or the
    // call fails (e.g. running inside a build pipeline or test runner).
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

    // Moves the cursor, ignoring failures that occur when output is redirected.
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

    // Displays an error message in red, restoring the previous foreground colour
    // afterwards so the caller's colour state is not disturbed.
    private void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }

    // Only disposes the ApiClient when this shell created it. Injected clients
    // are owned by the caller and must not be disposed here.
    public void Dispose()
    {
        if (_ownsApiClient)
        {
            _apiClient.Dispose();
        }
    }

    // Win32 API — retrieves a handle to the standard input stream.
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    // Win32 API — discards any unread input events from the console input buffer.
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);
}
