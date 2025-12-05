using System.Collections.Generic;
using BrickBreaker.Core.Clients;
using BrickBreaker.Core.Models;

namespace BrickBreaker.WinFormsClient.Hosting;

/// <summary>
/// Encapsulates the WinForms launcher state and API interactions so the UI layer stays thin.
/// </summary>
public sealed class LauncherShell : IDisposable
{
    private readonly GameApiClient _apiClient;

    private string? _currentPlayer;
    private bool _quickPlayMode;
    private int? _lastScore;
    private string _statusMessage = "Ready.";
    private bool _statusSuccess;

    public LauncherShell(GameApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string BaseAddress => _apiClient.BaseAddress;
    public bool QuickPlayMode => _quickPlayMode;
    public bool CanSubmitScores => !_quickPlayMode && !string.IsNullOrWhiteSpace(_currentPlayer);
    public string? CurrentPlayer => _currentPlayer;

    public LauncherViewState Snapshot() => new(
        PlayerLabel: _quickPlayMode
            ? "Mode: Quick Play"
            : string.IsNullOrWhiteSpace(_currentPlayer)
                ? "Player: (not logged in)"
                : $"Player: {_currentPlayer}",
        LastScoreLabel: _lastScore is { } s and > 0 ? $"Last score: {s}" : "Last score: none",
        CanStartGame: _quickPlayMode || !string.IsNullOrWhiteSpace(_currentPlayer),
        CanLogout: !_quickPlayMode && !string.IsNullOrWhiteSpace(_currentPlayer),
        IsQuickPlay: _quickPlayMode,
        StatusMessage: _statusMessage,
        StatusIsSuccess: _statusSuccess);

    public void SetBaseAddress(string baseAddress)
    {
        _apiClient.SetBaseAddress(baseAddress);
        UpdateStatus($"API base updated to {BaseAddress}", true);
    }

    public LauncherViewState EnableQuickPlay()
    {
        _quickPlayMode = true;
        _currentPlayer = null;
        _lastScore = null;
        UpdateStatus("Quick Play ready. Scores are not submitted.", true);
        return Snapshot();
    }

    public LauncherViewState Logout()
    {
        _quickPlayMode = false;
        _currentPlayer = null;
        _lastScore = null;
        UpdateStatus("Signed out.");
        return Snapshot();
    }

    public void UpdateStatus(string message, bool success = false)
    {
        _statusMessage = message;
        _statusSuccess = success;
    }

    public LauncherViewState RecordGameFinished(int score)
    {
        _lastScore = score > 0 ? score : null;
        if (_quickPlayMode)
        {
            UpdateStatus("Quick Play finished.");
        }
        else if (score > 0)
        {
            UpdateStatus($"Final score: {score}", true);
        }
        else
        {
            UpdateStatus("No score recorded.");
        }

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

    public async Task<ApiResult> LoginAsync(string username, string password)
    {
        UpdateStatus("Signing in...");
        var result = await _apiClient.LoginAsync(username, password);
        if (result.Success)
        {
            _currentPlayer = username;
            _quickPlayMode = false;
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
        if (result.Success)
        {
            UpdateStatus($"Leaderboard updated at {DateTime.Now:T}", true);
        }
        else
        {
            UpdateStatus(result.Error ?? "Failed to load leaderboard.");
        }

        return result;
    }

    public void Dispose()
    {
        _apiClient.Dispose();
    }
}
