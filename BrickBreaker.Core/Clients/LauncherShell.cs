
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Clients;

// Encapsulates launcher state and API interactions so the UI layer stays thin.
// All mutable state lives here; the form only reads snapshots and calls methods.
public sealed class LauncherShell : IDisposable
{
    private readonly GameApiClient _apiClient;

    // Session state — reset on logout or API base change.
    private string? _currentPlayer;
    private bool _quickPlayMode = true;
    private int? _bestScore;

    // Status bar text shown at the bottom of the leaderboard section.
    private string _statusMessage = "Ready.";
    private bool _statusSuccess;

    public LauncherShell(GameApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string BaseAddress => _apiClient.BaseAddress;
    public bool CanSubmitScores => !_quickPlayMode && !string.IsNullOrWhiteSpace(_currentPlayer);

    // Produces an immutable snapshot of the current state for the UI to render.
    public LauncherViewState Snapshot() => new(
        PlayerLabel: string.IsNullOrWhiteSpace(_currentPlayer)
            ? "Player: -"
            : $"Player: {_currentPlayer}",
        BestScoreLabel: _bestScore is { } s and > 0 ? $"Best score: {s}" : "Best score: -",
        CanStartGame: _quickPlayMode || !string.IsNullOrWhiteSpace(_currentPlayer),
        CanLogout: !_quickPlayMode && !string.IsNullOrWhiteSpace(_currentPlayer),
        IsQuickPlay: _quickPlayMode,
        StatusMessage: _statusMessage,
        StatusIsSuccess: _statusSuccess);

    // Switches the API target and resets session state — requires re-login.
    public void SetBaseAddress(string baseAddress)
    {
        _apiClient.SetBaseAddress(baseAddress);
        _apiClient.ClearAuthentication();
        _currentPlayer = null;
        _quickPlayMode = true;
        UpdateStatus($"API base updated to {BaseAddress}", true);
    }

    // Clears session state and returns to quick-play mode.
    public LauncherViewState Logout()
    {
        _quickPlayMode = true;
        _currentPlayer = null;
        _bestScore = null;
        _apiClient.ClearAuthentication();
        UpdateStatus("Signed out.");
        return Snapshot();
    }

    public void UpdateStatus(string message, bool success = false)
    {
        _statusMessage = message;
        _statusSuccess = success;
    }

    // Called when a game session ends. Updates the local best score and status message.
    public LauncherViewState RecordGameFinished(int score)
    {
        if (score > 0)
            _bestScore = Math.Max(score, _bestScore ?? 0);

        if (_quickPlayMode)
            UpdateStatus("Quick Play finished.");
        else if (score > 0)
            UpdateStatus($"Final score: {score}", true);
        else
            UpdateStatus("No score recorded.");

        return Snapshot();
    }

    public async Task<ApiResult> RegisterAsync(string username, string password)
    {
        UpdateStatus("Registering account...");
        var result = await _apiClient.RegisterAsync(username, password);
        UpdateStatus(result.Success
            ? "Registration successful. You can log in now."
            : result.Error ?? "Registration failed.", result.Success);
        return result;
    }

    public async Task<ApiResult<LoginSession>> LoginAsync(string username, string password)
    {
        UpdateStatus("Signing in...");
        var result = await _apiClient.LoginAsync(username, password);
        if (result.Success && result.Value is not null)
        {
            _currentPlayer = result.Value.Username;
            _quickPlayMode = false;
            // Seed the local best score from the server so it's accurate from the first game.
            var best = await _apiClient.GetBestAsync(_currentPlayer);
            _bestScore = best.Value?.Score;
            UpdateStatus("Logged in.", true);
        }
        else
        {
            UpdateStatus(result.Error ?? "Login failed.");
        }

        return result;
    }

    public async Task<ApiResult> SubmitScoreAsync(int score)
    {
        UpdateStatus("Submitting score...");
        var result = await _apiClient.SubmitScoreAsync(_currentPlayer!, score);
        UpdateStatus(result.Success ? "Score submitted!" : result.Error ?? "Score submission failed.", result.Success);
        return result;
    }

    public async Task<ApiResult<IReadOnlyList<ScoreEntry>>> LoadLeaderboardAsync(int count)
    {
        var result = await _apiClient.GetLeaderboardAsync(count);
        UpdateStatus(result.Success
            ? $"Leaderboard updated at {DateTime.Now:T}"
            : result.Error ?? "Failed to load leaderboard.", result.Success);
        return result;
    }

    public void Dispose()
    {
        _apiClient.Dispose();
    }
}
