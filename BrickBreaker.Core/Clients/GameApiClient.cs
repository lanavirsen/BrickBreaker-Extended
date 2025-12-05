using System.Net.Http.Json;
using System.Text.Json;
using BrickBreaker.Core.Models;

namespace BrickBreaker.Core.Clients;

public sealed class GameApiClient : IGameApiClient
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public GameApiClient(string? baseAddress = null, HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient is null;

        if (!string.IsNullOrWhiteSpace(baseAddress))
        {
            SetBaseAddress(baseAddress);
        }
    }

    public string BaseAddress => _httpClient.BaseAddress?.ToString() ?? string.Empty;

    public void SetBaseAddress(string baseAddress)
    {
        var normalized = ApiConfiguration.NormalizeBaseAddress(baseAddress);
        _httpClient.BaseAddress = new Uri(normalized, UriKind.Absolute);
    }

    public Task<ApiResult> RegisterAsync(string username, string password)
        => SendCredentialsAsync("register", username, password);

    public Task<ApiResult> LoginAsync(string username, string password)
        => SendCredentialsAsync("login", username, password);

    public async Task<ApiResult> SubmitScoreAsync(string username, int score)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "leaderboard/submit",
                new SubmitScoreRequest(username, score),
                _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Ok();
            }

            return ApiResult.Fail(await ExtractErrorAsync(response));
        }
        catch (Exception ex)
        {
            return ApiResult.Fail(ex.Message);
        }
    }

    public async Task<ApiResult<IReadOnlyList<ScoreEntry>>> GetLeaderboardAsync(int count)
    {
        try
        {
            var response = await _httpClient.GetAsync($"leaderboard/top?count={count}");
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<IReadOnlyList<ScoreEntry>>.Fail(await ExtractErrorAsync(response));
            }

            var payload = await response.Content.ReadFromJsonAsync<List<ScoreEntry>>(_jsonOptions)
                          ?? new List<ScoreEntry>();
            return ApiResult<IReadOnlyList<ScoreEntry>>.Ok(payload);
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<ScoreEntry>>.Fail(ex.Message);
        }
    }

    public async Task<ApiResult<ScoreEntry?>> GetBestAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"leaderboard/best/{Uri.EscapeDataString(username)}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return ApiResult<ScoreEntry?>.Ok(null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<ScoreEntry?>.Fail(await ExtractErrorAsync(response));
            }

            var payload = await response.Content.ReadFromJsonAsync<ScoreEntry>(_jsonOptions);
            return ApiResult<ScoreEntry?>.Ok(payload);
        }
        catch (Exception ex)
        {
            return ApiResult<ScoreEntry?>.Fail(ex.Message);
        }
    }

    private async Task<ApiResult> SendCredentialsAsync(string path, string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                path,
                new CredentialRequest(username, password),
                _jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return ApiResult.Ok();
            }

            return ApiResult.Fail(await ExtractErrorAsync(response));
        }
        catch (Exception ex)
        {
            return ApiResult.Fail(ex.Message);
        }
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(body))
        {
            return body.Trim();
        }

        return $"API call failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).";
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private sealed record CredentialRequest(string Username, string Password);
    private sealed record SubmitScoreRequest(string Username, int Score);
}
