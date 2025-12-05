using BrickBreaker.ConsoleClient.Game;
using BrickBreaker.ConsoleClient.Ui;
using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using BrickBreaker.ConsoleClient.WebApi;
using Spectre.Console;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private const string DefaultApiBase = "http://127.0.0.1:5080";

    private static string? currentUser;
    private static bool _isLoggedIn;
    private static ConsoleApiClient? _api;

    static ILoginMenu _loginMenu = new LoginMenu();
    static IGameplayMenu _gameplayMenu = new GameplayMenu();
    static IConsoleDialogs _dialogs = new ConsoleDialogs();
    static GameMode currentMode = GameMode.Normal;

    static Header header = new Header();

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var apiBase = Environment.GetEnvironmentVariable("BRICKBREAKER_API_URL") ?? DefaultApiBase;
        try
        {
            _api = new ConsoleApiClient(apiBase);
        }
        catch (Exception ex)
        {
            ShowDatabaseWarning($"Failed to initialize API client: {ex.Message}");
        }

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

    static async Task<AppState> HandleLoginMenuAsync()
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

        static AppState SetQuickPlayAndStart()
        {
            ClearInputBuffer();
            currentMode = GameMode.QuickPlay;
            return AppState.Playing;
        }
    }

    static async Task<AppState> HandleRegisterAsync()
    {
        await DoRegisterAsync();
        _dialogs.Pause();
        return AppState.LoginMenu;
    }

    static async Task<AppState> HandleLeaderboardAsync()
    {
        await ShowLeaderboardAsync();
        _dialogs.Pause();
        return AppState.LoginMenu;
    }

    static async Task<AppState> HandleGameplayMenuAsync()
    {
        var choice = _gameplayMenu.Show(currentUser ?? "guest");

        return choice switch
        {
            GameplayMenuChoice.Start => StartNormalPlay(),
            GameplayMenuChoice.Best => await ShowBestScoreAsyncAndStay(),
            GameplayMenuChoice.Leaderboard => await DisplayLeaderboardAndStayAsync(),
            GameplayMenuChoice.Logout => Logout(),
            _ => AppState.Exit
        };

        static AppState StartNormalPlay()
        {
            currentMode = GameMode.Normal;
            return AppState.Playing;
        }

        static async Task<AppState> ShowBestScoreAsyncAndStay()
        {
            await ShowBestScoreAsync();
            return AppState.GameplayMenu;
        }

        static async Task<AppState> DisplayLeaderboardAndStayAsync()
        {
            await ShowLeaderboardAsync();
            _dialogs.Pause();
            return AppState.GameplayMenu;
        }

        static AppState Logout()
        {
            currentUser = null;
            _isLoggedIn = false;
            return AppState.LoginMenu;
        }
    }

    static async Task<AppState> HandlePlayingAsync()
    {
        AnsiConsole.Clear();
        IGame game = new BrickBreakerGame();
        int score = game.Run();

        int lowerLine = Console.WindowHeight - 4;
        if (lowerLine < 0) lowerLine = 0;
        Console.SetCursorPosition(0, lowerLine);

        _dialogs.ShowMessage($"\nFinal score: {score}");
        if (currentMode != GameMode.QuickPlay && _isLoggedIn && _api is not null)
        {
            await RunWithSpinnerAsync("Submitting score...", () => _api.SubmitScoreAsync(currentUser!, score));
        }

        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    static async Task DoRegisterAsync()
    {
        if (_api is null)
        {
            ShowDatabaseWarning("API base URL not configured.");
            return;
        }

        var username = _dialogs.PromptNewUsername();
        var password = _dialogs.PromptNewPassword();

        var result = await RunWithSpinnerAsync("Registering account...", () => _api.RegisterAsync(username, password));
        _dialogs.ShowMessage(result.Success
            ? "Registration successful!"
            : result.Error ?? "Registration failed.");
    }

    static async Task<bool> DoLoginAsync()
    {
        if (_api is null)
        {
            ShowDatabaseWarning("API base URL not configured.");
            return false;
        }

        var (username, password) = _dialogs.PromptCredentials();

        var result = await RunWithSpinnerAsync("Signing in...", () => _api.LoginAsync(username, password));
        if (result.Success)
        {
            currentUser = username;
            _isLoggedIn = true;
            return true;
        }

        _dialogs.ShowMessage(result.Error ?? "Login failed (wrong username or password).");
        return false;
    }

    static async Task ShowLeaderboardAsync()
    {
        AnsiConsole.Clear();
        header.TitleHeader();

        if (_api is null)
        {
            ShowDatabaseWarning("Leaderboard is unavailable because the API base URL is not configured.");
            return;
        }

        var result = await RunWithSpinnerAsync("Loading leaderboard...", () => _api.GetLeaderboardAsync(10));
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

        var items = result.Value.Select(s => (s.Username, s.Score, s.At));
        _dialogs.ShowLeaderboard(items);
    }

    static async Task ShowBestScoreAsync()
    {
        if (_api is null || !_isLoggedIn || string.IsNullOrWhiteSpace(currentUser))
        {
            ShowDatabaseWarning("Best score lookup requires an active API connection and login.");
            return;
        }

        var result = await RunWithSpinnerAsync("Fetching best score...", () => _api.GetBestAsync(currentUser!));
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

    const int STD_INPUT_HANDLE = -10;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);

    static void ClearInputBuffer()
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }

        try
        {
            var handle = GetStdHandle(STD_INPUT_HANDLE);
            if (handle != IntPtr.Zero)
            {
                FlushConsoleInputBuffer(handle);
            }
        }
        catch
        {
        }
    }

    static void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }

    static async Task<T> RunWithSpinnerAsync<T>(string description, Func<Task<T>> action)
    {
        T result = default!;
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(description, async _ => result = await action());
        return result;
    }
}
