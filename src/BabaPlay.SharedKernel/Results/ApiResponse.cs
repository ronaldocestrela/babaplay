namespace BabaPlay.SharedKernel.Results;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<object?> OkEmpty() =>
        new() { Success = true, Data = null };

    public static ApiResponse<T> Fail(string? error, IReadOnlyList<string>? errors = null) =>
        new()
        {
            Success = false,
            Error = error ?? errors?.FirstOrDefault(),
            Errors = errors is { Count: > 0 } ? errors : error is not null ? new[] { error } : Array.Empty<string>()
        };
}
