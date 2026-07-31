namespace BabaPlay.Web.Models;

public sealed class ApiServiceResult<T>
{
    public T? Data { get; init; }
    public int? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsSuccess => StatusCode is >= 200 and < 300;

    public static ApiServiceResult<T> Ok(T data, int statusCode = 200) => new()
    {
        Data = data,
        StatusCode = statusCode,
    };

    public static ApiServiceResult<T> Fail(int statusCode, string errorMessage) => new()
    {
        StatusCode = statusCode,
        ErrorMessage = errorMessage,
    };
}
