using System.Collections.Generic;
using System.Threading.Tasks;
using BrickBreaker.ConsoleClient.Shell;
using BrickBreaker.ConsoleClient.Ui.Enums;
using BrickBreaker.ConsoleClient.Ui.Interfaces;
using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Tests.Console;

public class ConsoleShellTests
{
    [Fact]
    public async Task QuickPlay_DoesNotSubmitScores()
    {
        var deps = new ConsoleShellDependencies
        {
            LoginMenu = new FakeLoginMenu(LoginMenuChoice.QuickPlay, LoginMenuChoice.Exit),
            GameplayMenu = new FakeGameplayMenu(GameplayMenuChoice.Logout),
            Dialogs = new FakeDialogs(),
            Renderer = new FakeRenderer(),
            ApiClient = new FakeApiClient(),
            GameHost = new FakeGameHost(150),
            OwnsApiClient = false
        };

        using var shell = new ConsoleShell(deps);
        await shell.RunAsync();

        Assert.Equal(1, ((FakeGameHost)deps.GameHost).RunCount);
        Assert.Equal(0, ((FakeApiClient)deps.ApiClient).SubmitScoreCalls);
    }

    [Fact]
    public async Task NormalMode_SubmitsScoreAfterLogin()
    {
        var dialogs = new FakeDialogs();
        dialogs.Credentials.Enqueue(("tester", "secret"));

        var apiClient = new FakeApiClient
        {
            LoginResult = ApiResult<LoginSession>.Ok(new LoginSession("tester"))
        };

        var deps = new ConsoleShellDependencies
        {
            LoginMenu = new FakeLoginMenu(LoginMenuChoice.Login, LoginMenuChoice.Exit),
            GameplayMenu = new FakeGameplayMenu(GameplayMenuChoice.Start, GameplayMenuChoice.Logout),
            Dialogs = dialogs,
            Renderer = new FakeRenderer(),
            ApiClient = apiClient,
            GameHost = new FakeGameHost(420),
            OwnsApiClient = false
        };

        using var shell = new ConsoleShell(deps);
        await shell.RunAsync();

        Assert.Equal(1, apiClient.SubmitScoreCalls);
        Assert.Equal(("tester", 420), apiClient.LastScoreSubmission);
    }

    private sealed class FakeLoginMenu : ILoginMenu
    {
        private readonly Queue<LoginMenuChoice> _choices;

        public FakeLoginMenu(params LoginMenuChoice[] choices)
        {
            _choices = new Queue<LoginMenuChoice>(choices);
        }

        public LoginMenuChoice Show() => _choices.Count > 0 ? _choices.Dequeue() : LoginMenuChoice.Exit;
    }

    private sealed class FakeGameplayMenu : IGameplayMenu
    {
        private readonly Queue<GameplayMenuChoice> _choices;

        public FakeGameplayMenu(params GameplayMenuChoice[] choices)
        {
            _choices = new Queue<GameplayMenuChoice>(choices);
        }

        public GameplayMenuChoice Show(string username) => _choices.Count > 0 ? _choices.Dequeue() : GameplayMenuChoice.Logout;
    }

    private sealed class FakeDialogs : IConsoleDialogs
    {
        public Queue<(string Username, string Password)> Credentials { get; } = new();

        public (string Username, string Password) PromptCredentials()
            => Credentials.Count > 0 ? Credentials.Dequeue() : ("user", "pass");

        public string PromptNewUsername() => "new-user";

        public string PromptNewPassword() => "new-pass";

        public void ShowMessage(string message) { }

        public void Pause() { }

        public void ShowLeaderboard(IEnumerable<(string Username, int Score, DateTimeOffset At)> entries) { }
    }

    private sealed class FakeRenderer : IConsoleRenderer
    {
        public void ClearScreen() { }

        public void RenderHeader() { }

        public Task<T> RunStatusAsync<T>(string description, Func<Task<T>> action) => action();
    }

    private sealed class FakeApiClient : IGameApiClient
    {
        public string BaseAddress { get; private set; } = "http://localhost/";
        public ApiResult<LoginSession> LoginResult { get; set; } = ApiResult<LoginSession>.Ok(new LoginSession("tester"));
        public ApiResult RegisterResult { get; set; } = ApiResult.Ok();
        public ApiResult SubmitScoreResult { get; set; } = ApiResult.Ok();
        public ApiResult<IReadOnlyList<ScoreEntry>> LeaderboardResult { get; set; } = ApiResult<IReadOnlyList<ScoreEntry>>.Ok(Array.Empty<ScoreEntry>());
        public ApiResult<ScoreEntry?> BestResult { get; set; } = ApiResult<ScoreEntry?>.Ok(null);
        public int SubmitScoreCalls { get; private set; }
        public (string Username, int Score)? LastScoreSubmission { get; private set; }

        public void SetBaseAddress(string baseAddress) => BaseAddress = baseAddress;

        public Task<ApiResult> RegisterAsync(string username, string password) => Task.FromResult(RegisterResult);

        public Task<ApiResult<LoginSession>> LoginAsync(string username, string password) => Task.FromResult(LoginResult);

        public Task<ApiResult> SubmitScoreAsync(string username, int score)
        {
            SubmitScoreCalls++;
            LastScoreSubmission = (username, score);
            return Task.FromResult(SubmitScoreResult);
        }

        public Task<ApiResult<IReadOnlyList<ScoreEntry>>> GetLeaderboardAsync(int count) => Task.FromResult(LeaderboardResult);

        public Task<ApiResult<ScoreEntry?>> GetBestAsync(string username) => Task.FromResult(BestResult);

        public void ClearAuthentication() { }

        public void Dispose() { }
    }

    private sealed class FakeGameHost : IGameHost
    {
        private readonly int _score;
        public int RunCount { get; private set; }

        public FakeGameHost(int score)
        {
            _score = score;
        }

        public int Run()
        {
            RunCount++;
            return _score;
        }
    }
}
