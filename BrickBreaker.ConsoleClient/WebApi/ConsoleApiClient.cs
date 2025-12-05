using System.Net.Http.Json;
using System.Text.Json;

namespace BrickBreaker.ConsoleClient.WebApi;

public sealed class ConsoleApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ConsoleApiClient(string baseAddress)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseAddress, UriKind.Absolute)
        };
    }

    public async Task<ApiResult> LoginAsync(string username, string password)
    {
        return await SendCredentialsAsync("login", username, password);
    }

    public async Task<ApiResult> RegisterAsync(string username, string password)
    {
        return await SendCredentialsAsync("register", username, password);
    }

    public async Task<ApiResult> SubmitScoreAsync(string username, int score)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("leaderboard/submit", new SubmitScoreRequest(username, score), _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Ok();
            }
            return ApiResult.Fail(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return ApiResult.Fail(ex.Message);
        }
    }

    public async Task<ApiResult<IReadOnlyList<LeaderboardEntry>>> GetLeaderboardAsync(int count)
    {
        try
        {
            var response = await _httpClient.GetAsync($"leaderboard/top?count={count}");
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<IReadOnlyList<LeaderboardEntry>>.Fail(await response.Content.ReadAsStringAsync());
            }

            var payload = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>(_jsonOptions) ?? new List<LeaderboardEntry>();
            return ApiResult<IReadOnlyList<LeaderboardEntry>>.Ok(payload);
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<LeaderboardEntry>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResult<LeaderboardEntry?>> GetBestAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"leaderboard/best/{Uri.EscapeDataString(username)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return ApiResult<LeaderboardEntry?>.Ok(null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<LeaderboardEntry?>.Fail(await response.Content.ReadAsStringAsync());
            }

            var payload = await response.Content.ReadFromJsonAsync<LeaderboardEntry>(_jsonOptions);
            return ApiResult<LeaderboardEntry?>.Ok(payload);
        }
        catch (Exception ex)
        {
            return ApiResult<LeaderboardEntry?>.Fail(ex.Message);
        }
    }

    private async Task<ApiResult> SendCredentialsAsync(string path, string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, new CredentialRequest(username, password), _jsonOptions);
            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Ok();
            }

            return ApiResult.Fail(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            return ApiResult.Fail(ex.Message);
        }
    }

    private record CredentialRequest(string Username, string Password);
    private record SubmitScoreRequest(string Username, int Score);
}

public readonly record struct ApiResult(bool Success, string? Error)
{
    public static ApiResult Ok() => new(true, null);
    public static ApiResult Fail(string? error) => new(false, string.IsNullOrWhiteSpace(error) ? "Unknown error" : error);
}

public readonly record struct ApiResult<T>(bool Success, string? Error, T? Value)
{
    public static ApiResult<T> Ok(T value) => new(true, null, value);
    public static ApiResult<T> Fail(string? error) => new(false, string.IsNullOrWhiteSpace(error) ? "Unknown error" : error, default);
}

public sealed record LeaderboardEntry(string Username, int Score, DateTimeOffset At);
