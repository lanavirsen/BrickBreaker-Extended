using BrickBreaker.ConsoleClient.Game;
using BrickBreaker.ConsoleClient.Ui;
using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.ConsoleClient.Ui.SpecterConsole;
using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Services;
using BrickBreaker.Storage;
using Spectre.Console;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static string? currentUser = null;
    private static ILeaderboardService _leaderboard = null!;
    private static IAuthService _auth = null!;
    private static bool _databaseAvailable = false;

    // UI menus and dialogs 
    static ILoginMenu _loginMenu = new LoginMenu();
    static IGameplayMenu _gameplayMenu = new GameplayMenu();
    static IConsoleDialogs _dialogs = new ConsoleDialogs();
    static GameMode currentMode = GameMode.Normal;

    static Header header = new Header();

    static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var storageConfig = new StorageConfiguration();
        var connectionString = storageConfig.GetConnectionString();

        IUserStore userStore;
        ILeaderboardStore leaderboardStore;

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            userStore = new UserStore(connectionString);
            leaderboardStore = new LeaderboardStore(connectionString);
            _databaseAvailable = true;
        }
        else
        {
            userStore = new DisabledUserStore();
            leaderboardStore = new DisabledLeaderboardStore();
            _databaseAvailable = false;
            ShowDatabaseWarning("Supabase connection string missing. Database features are disabled until it is configured.");
        }

        _leaderboard = new LeaderboardService(leaderboardStore);
        _auth = new AuthService(userStore);

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
        if (currentMode != GameMode.QuickPlay && _databaseAvailable)
        {
            await _leaderboard.SubmitAsync(currentUser ?? "guest", score);
        }

        _dialogs.Pause();
        return AppState.GameplayMenu;
    }

    static async Task DoRegisterAsync()
    {
        var username = _dialogs.PromptNewUsername();
        var password = _dialogs.PromptNewPassword();

        bool registered = await RunWithSpinnerAsync("Registering account...", () => _auth.RegisterAsync(username, password));
        _dialogs.ShowMessage(registered
            ? "Registration successful!"
            : "Registration failed. Username might already exist.");
    }

    static async Task<bool> DoLoginAsync()
    {
        var (username, password) = _dialogs.PromptCredentials();

        bool loggedIn = await RunWithSpinnerAsync("Signing in...", () => _auth.LoginAsync(username, password));
        if (loggedIn)
        {
            currentUser = username;
            return true;
        }

        _dialogs.ShowMessage("Login failed (wrong username or password).");
        return false;
    }

    static async Task ShowLeaderboardAsync()
    {
        AnsiConsole.Clear();
        header.TitleHeader();

        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Leaderboard is unavailable because the Supabase connection string is missing.");
            return;
        }

        var top = await RunWithSpinnerAsync("Loading leaderboard...", () => _leaderboard.TopAsync(10));
        if (!top.Any())
        {
            _dialogs.ShowMessage("\nTop 10 leaderboard:\nNo scores yet.");
            return;
        }

        var items = top.Select(s => (s.Username, s.Score, s.At));
        _dialogs.ShowLeaderboard(items);
    }

    static async Task ShowBestScoreAsync()
    {
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Best score lookup requires the Supabase database. Please configure the connection string first.");
            return;
        }

        var best = await RunWithSpinnerAsync("Fetching best score...", () => _leaderboard.BestForAsync(currentUser ?? string.Empty));
        if (best is null)
        {
            _dialogs.ShowMessage("\nNo scores recorded yet.");
        }
        else
        {
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
            // ignore for non-Windows terminals
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
