namespace BrickBreaker.Core.Clients;

public readonly record struct ApiResult(bool Success, string? Error)
{
    public static ApiResult Ok() => new(true, null);
    public static ApiResult Fail(string? error) => new(false, Normalize(error));

    private static string? Normalize(string? error)
        => string.IsNullOrWhiteSpace(error) ? "Unknown error." : error;
}

public readonly record struct ApiResult<T>(bool Success, string? Error, T? Value)
{
    public static ApiResult<T> Ok(T value) => new(true, null, value);
    public static ApiResult<T> Fail(string? error) => new(false, Normalize(error), default);

    private static string? Normalize(string? error)
        => string.IsNullOrWhiteSpace(error) ? "Unknown error." : error;
}
