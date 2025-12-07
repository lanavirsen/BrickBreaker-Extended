using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BrickBreaker.WebClient.Services;

public sealed class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string BaseAddress => _httpClient.BaseAddress?.ToString() ?? string.Empty;

    public void SetBaseAddress(string baseAddress)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new ArgumentException("Base address cannot be empty.", nameof(baseAddress));
        }

        if (!baseAddress.EndsWith('/'))
        {
            baseAddress += "/";
        }

        _httpClient.BaseAddress = new Uri(baseAddress, UriKind.Absolute);
        ClearAuthentication();
    }

    public void ClearAuthentication() => _httpClient.DefaultRequestHeaders.Authorization = null;

    public void SetAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            ClearAuthentication();
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ApiResult> RegisterAsync(string username, string password, string? turnstileToken = null)
    {
        return await SendCredentialRequestAsync("register", username, password, turnstileToken);
    }

    public async Task<ApiResult<LoginPayload>> LoginAsync(string username, string password, string? turnstileToken = null)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("login", new CredentialRequest(username, password, turnstileToken), _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<LoginPayload>.Fail(await ExtractErrorAsync(response));
            }

            var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
            if (payload is null || string.IsNullOrWhiteSpace(payload.Token) || string.IsNullOrWhiteSpace(payload.Username))
            {
                return ApiResult<LoginPayload>.Fail("Login response missing token.");
            }

            SetAccessToken(payload.Token);
            return ApiResult<LoginPayload>.Ok(new LoginPayload(payload.Username));
        }
        catch (Exception ex)
        {
            return ApiResult<LoginPayload>.Fail(ex.Message);
        }
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

    public async Task<ApiResult<LeaderboardEntry?>> GetBestAsync(string username)
    {
        try
        {
            var response = await _httpClient.GetAsync($"leaderboard/best/{Uri.EscapeDataString(username)}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return ApiResult<LeaderboardEntry?>.Ok(null);
            }

            if (!response.IsSuccessStatusCode)
            {
                return ApiResult<LeaderboardEntry?>.Fail(await ExtractErrorAsync(response));
            }

            var entry = await response.Content.ReadFromJsonAsync<LeaderboardEntry>(_jsonOptions);
            return ApiResult<LeaderboardEntry?>.Ok(entry);
        }
        catch (Exception ex)
        {
            return ApiResult<LeaderboardEntry?>.Fail(ex.Message);
        }
    }

    private async Task<ApiResult> SendCredentialRequestAsync(string path, string username, string password, string? turnstileToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(path, new CredentialRequest(username, password, turnstileToken), _jsonOptions);
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

    private record CredentialRequest(string Username, string Password, string? TurnstileToken);
    private record SubmitScoreRequest(string Username, int Score);
    private record LoginResponse(string Username, string Token);

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(body))
        {
            return body.Trim();
        }

        return $"API call failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).";
    }
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
public sealed record LoginPayload(string Username);
