using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace BrickBreaker.WinFormsClient.Services;

public sealed class BrickBreakerApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public BrickBreakerApiClient(string baseAddress)
    {
        _httpClient = new HttpClient();
        SetBaseAddress(baseAddress);
    }

    public string BaseAddress => _httpClient.BaseAddress?.ToString() ?? string.Empty;

    public void SetBaseAddress(string baseAddress)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new ArgumentException("API base address cannot be empty.", nameof(baseAddress));
        }

        if (!baseAddress.EndsWith("/", StringComparison.Ordinal))
        {
            baseAddress += "/";
        }

        _httpClient.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
    }

    public Task<ApiResult> RegisterAsync(string username, string password)
        => SendCredentialRequestAsync("register", username, password);

    public Task<ApiResult> LoginAsync(string username, string password)
        => SendCredentialRequestAsync("login", username, password);

    public async Task<ApiResult> SubmitScoreAsync(string username, int score)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("leaderboard/submit", new SubmitScoreRequest(username, score), _jsonOptions);
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

    public async Task<ApiResult<IReadOnlyList<LeaderboardEntry>>> GetLeaderboardAsync(int count)
    {
        try
        {
            var response = await _httpClient.GetAsync($"leaderboard/top?count={count}");
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<IReadOnlyList<LeaderboardEntry>>.Fail(await ExtractErrorAsync(response));
            }

            var payload = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>(_jsonOptions) ?? new List<LeaderboardEntry>();
            return ApiResult<IReadOnlyList<LeaderboardEntry>>.Ok(payload);
        }
        catch (Exception ex)
        {
            return ApiResult<IReadOnlyList<LeaderboardEntry>>.Fail(ex.Message);
        }
    }

    private async Task<ApiResult> SendCredentialRequestAsync(string path, string username, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, new CredentialRequest(username, password), _jsonOptions);
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
        _httpClient.Dispose();
    }

    private record CredentialRequest(string Username, string Password);
    private record SubmitScoreRequest(string Username, int Score);
}

public readonly record struct ApiResult(bool Success, string? ErrorMessage)
{
    public static ApiResult Ok() => new(true, null);
    public static ApiResult Fail(string? error) => new(false, string.IsNullOrWhiteSpace(error) ? "Unknown error." : error);
}

public readonly record struct ApiResult<T>(bool Success, string? ErrorMessage, T? Value)
{
    public static ApiResult<T> Ok(T value) => new(true, null, value);
    public static ApiResult<T> Fail(string? error) => new(false, string.IsNullOrWhiteSpace(error) ? "Unknown error." : error, default);
}

public sealed record LeaderboardEntry(string Username, int Score, DateTimeOffset At);
