namespace BabaPlay.Application.Common;

/// <summary>
/// Represents the result of an operation with a typed payload.
/// </summary>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string errorCode, string errorMessage)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string errorCode, string errorMessage) => new(errorCode, errorMessage);
}

/// <summary>
/// Represents the result of a command that returns no payload (void equivalent).
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result() => IsSuccess = true;

    private Result(string errorCode, string errorMessage)
    {
        IsSuccess = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new();
    public static Result Fail(string errorCode, string errorMessage) => new(errorCode, errorMessage);

    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(string errorCode, string errorMessage) => Result<T>.Fail(errorCode, errorMessage);
}
