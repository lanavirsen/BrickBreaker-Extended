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

    public LauncherShell(GameApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public string? CurrentPlayer { get; private set; }
    public bool QuickPlayMode { get; private set; }
    public string BaseAddress => _apiClient.BaseAddress;

    public void SetBaseAddress(string baseAddress)
    {
        _apiClient.SetBaseAddress(baseAddress);
    }

    public void EnableQuickPlay()
    {
        QuickPlayMode = true;
        CurrentPlayer = null;
    }

    public void Logout()
    {
        QuickPlayMode = false;
        CurrentPlayer = null;
    }

    public async Task<ApiResult> RegisterAsync(string username, string password)
    {
        return await _apiClient.RegisterAsync(username, password);
    }

    public async Task<ApiResult> LoginAsync(string username, string password)
    {
        var result = await _apiClient.LoginAsync(username, password);
        if (result.Success)
        {
            QuickPlayMode = false;
            CurrentPlayer = username;
        }

        return result;
    }

    public Task<ApiResult> SubmitScoreAsync(int score)
        => _apiClient.SubmitScoreAsync(CurrentPlayer!, score);

    public Task<ApiResult<IReadOnlyList<ScoreEntry>>> LoadLeaderboardAsync(int count)
        => _apiClient.GetLeaderboardAsync(count);

    public void Dispose()
    {
        _apiClient.Dispose();
    }
}
