using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Clients;

public interface IGameApiClient : IDisposable
{
    string BaseAddress { get; }
    void SetBaseAddress(string baseAddress);
    void ClearAuthentication();

    Task<ApiResult> RegisterAsync(string username, string password);
    Task<ApiResult<LoginSession>> LoginAsync(string username, string password);
    Task<ApiResult> SubmitScoreAsync(string username, int score);
    Task<ApiResult<IReadOnlyList<ScoreEntry>>> GetLeaderboardAsync(int count);
    Task<ApiResult<ScoreEntry?>> GetBestAsync(string username);
}
