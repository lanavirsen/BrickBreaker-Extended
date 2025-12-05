using BrickBreaker.Core.Abstractions;
using BrickBreaker.Core.Services;
using BrickBreaker.Game;
using BrickBreaker.Storage;
using BrickBreaker.Ui;
using BrickBreaker.UI.Ui.Enums;
using BrickBreaker.UI.Ui.Interfaces;
using BrickBreaker.UI.Ui.SpecterConsole;
using Spectre.Console;
#if WINDOWS
using BrickBreaker.WinFormsClient.Hosting;
#endif
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

static Header header = new Header();
static GameMode currentMode = GameMode.Normal;

    // Application entry point
    static async Task Main()
    {
        // Ensure UTF-8 encoding for console output
        Console.OutputEncoding = Encoding.UTF8;

        var storageConfig = new StorageConfiguration();
        // Get the connection string for the database
        var connectionString = storageConfig.GetConnectionString();

        IUserStore userStore;
        ILeaderboardStore leaderboardStore;

        // Initialize database stores based on the connection string
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            userStore = new UserStore(connectionString);
            leaderboardStore = new LeaderboardStore(connectionString);
            _databaseAvailable = true;
        }
        else
        {
            // Fallback to disabled stores if connection string is missing
            userStore = new DisabledUserStore();
            leaderboardStore = new DisabledLeaderboardStore();
            // Set database availability flag
            _databaseAvailable = false;
            // Show warning about missing database configuration
            ShowDatabaseWarning("Supabase connection string missing. Database features are disabled until it is configured.");
        }

        _leaderboard = new LeaderboardService(leaderboardStore);
        _auth = new AuthService(userStore);

        // Main application loop
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

    // Login Menu Handler
    static async Task<AppState> HandleLoginMenuAsync()
    {
        var choice = _loginMenu.Show();

        switch (choice)
        {
            case LoginMenuChoice.QuickPlay:
                ClearInputBuffer();
                currentMode = GameMode.QuickPlay;
                return AppState.Playing;
            case LoginMenuChoice.Register:
                await DoRegisterAsync();
                _dialogs.Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Login:
                return await DoLoginAsync() ? AppState.GameplayMenu : AppState.LoginMenu;

            case LoginMenuChoice.Leaderboard:
                await ShowLeaderboardAsync();
                _dialogs.Pause();
                return AppState.LoginMenu;

            case LoginMenuChoice.Exit:
                return AppState.Exit;

            default:
                return AppState.LoginMenu;
        }
    }


    // Gameplay Menu Handler
    static async Task<AppState> HandleGameplayMenuAsync()
    {
        GameplayMenuChoice choice = _gameplayMenu.Show(currentUser ?? "guest");

        switch (choice)
        {
            case GameplayMenuChoice.Start:
                currentMode = GameMode.Normal;
                return AppState.Playing;

            case GameplayMenuChoice.Best:
                await ShowBestScoreAsync();
                return AppState.GameplayMenu;

            case GameplayMenuChoice.Leaderboard:
                await ShowLeaderboardAsync();
                _dialogs.Pause();
                return AppState.GameplayMenu;

            case GameplayMenuChoice.Logout:
                currentUser = null;
                return AppState.LoginMenu;

            default:
                return AppState.Exit;
        }
    }

    // Playing Handler
    static async Task<AppState> HandlePlayingAsync()
    {
#if !WINDOWS
        _dialogs.ShowMessage("WinForms gameplay is only available on Windows.");
        _dialogs.Pause();
        return AppState.GameplayMenu;
#else
        AnsiConsole.Clear();
        IGame game = new WinFormsBrickBreakerGame();
        int score = await Task.Run(() => game.Run());

        int lowerLine = Console.WindowHeight - 4;
        Console.SetCursorPosition(0, Math.Max(lowerLine, 0));

        _dialogs.ShowMessage($"\nFinal score: {score}");
        if (currentMode != GameMode.QuickPlay)
        {
            if (_databaseAvailable)
            {
                await _leaderboard.SubmitAsync(currentUser ?? "guest", score);
            }
            else
            {
                ShowDatabaseWarning("Unable to submit scores without the Supabase connection string. Your score was not saved.");
            }
        }

        _dialogs.Pause();

        return currentUser is null ? AppState.LoginMenu : AppState.GameplayMenu;
#endif
    }

    // Helper methods
    static async Task DoRegisterAsync()
    {
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Registration is disabled because the Supabase connection string is missing.");
            return;
        }

        var username = _dialogs.PromptNewUsername();

        username = (username ?? "").Trim();

        // Checks so username is not empty 
        if (username.Length == 0)
        {
            _dialogs.ShowMessage("Username can't be empty.");
            return;
        }

        var exists = await _auth.UsernameExistsAsync(username);
        if (exists)
        {
            _dialogs.ShowMessage("Username already exists.");
            return;
        }

        var password = _dialogs.PromptNewPassword();

        // Attempt to register the new user/ add new user to the database
        bool ok = await _auth.RegisterAsync(username, password);
        _dialogs.ShowMessage(ok
            ? "Registration successful! You can now log in."
            : "Registration failed (empty or already exists).");
    }

    // Login helper method
    static async Task<bool> DoLoginAsync()
    {
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Login requires the Supabase database. Please configure the connection string first.");
            return false;
        }

        var (username, password) = _dialogs.PromptCredentials();

        if (await _auth.LoginAsync(username, password))
        {
            currentUser = username;

            //Loading bar animation AFTER successful login
            AnsiConsole.Progress()
                .Columns(
                    new ProgressColumn[]
                    {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new SpinnerColumn()
                    })
                // Start the progress display
                .Start(ctx =>
                {
                    var verifyTask = ctx.AddTask("[yellow]Verifying user[/]");
                    var loadTask = ctx.AddTask("[green]Loading game data[/]");

                    while (!ctx.IsFinished)
                    {
                        verifyTask.Increment(5);
                        loadTask.Increment(4);
                        Thread.Sleep(40);
                    }
                });

            return true;
        }
        _dialogs.ShowMessage("Login failed (wrong username or password).");
        return false;
    }

    // Leaderboard display method
    static async Task ShowLeaderboardAsync()
    {
        AnsiConsole.Progress()
        .Columns(new ProgressColumn[]
        {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
        })

        .Start(ctx =>
            {
                var task = ctx.AddTask("[green]Loading Scores[/]", maxValue: 100);
                while (!ctx.IsFinished)
                {
                    task.Increment(4);
                    Thread.Sleep(40);
                }
            });


        AnsiConsole.Clear();
        header.TitleHeader();

        // Check database availability
        if (!_databaseAvailable)
        {
            ShowDatabaseWarning("Leaderboard is unavailable because the Supabase connection string is missing.");
            return;
        }

        var top = await _leaderboard.TopAsync(10);

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

        var best = await _leaderboard.BestForAsync(currentUser!);

        if (best == null)
        {
            _dialogs.ShowMessage("\nNo scores recorded yet.");
        }
        else
        {
            _dialogs.ShowMessage(
                $"\nYour best score: {best.Score} on {best.At.ToLocalTime():yyyy-MM-dd HH:mm}"
            );
        }
        _dialogs.Pause();
    }

    // Input buffer helper
    const int STD_INPUT_HANDLE = -10;

    // gets the standard input handle for the console
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);


    // clears the console input buffer, to avoid leftover key presses affecting subsequent input
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FlushConsoleInputBuffer(IntPtr hConsoleInput);

    // Clears any pending input in the console input buffer
    static void ClearInputBuffer()
    {
        while (Console.KeyAvailable)
            Console.ReadKey(true);

        try
        {
            var handle = GetStdHandle(STD_INPUT_HANDLE);
            if (handle != IntPtr.Zero)
                FlushConsoleInputBuffer(handle);
        }
        catch
        {
            // ignored: console might not be Windows or handle unavailable
        }
    }

    // Database warning display method
    static void ShowDatabaseWarning(string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        _dialogs.ShowMessage(message);
        Console.ForegroundColor = previousColor;
    }
}
