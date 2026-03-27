namespace BabaPlay.SharedKernel.Results;

public class Result
{
    protected Result(bool isSuccess, ResultStatus status, string? error, IReadOnlyList<string>? errors)
    {
        IsSuccess = isSuccess;
        Status = status;
        Error = error;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IReadOnlyList<string> Errors { get; }
    public ResultStatus Status { get; }

    public static Result Success() => new(true, ResultStatus.Ok, null, null);

    public static Result Failure(string message, ResultStatus status = ResultStatus.Error) =>
        new(false, status, message, new[] { message });

    public static Result<T> Success<T>(T value) => new(value, true, ResultStatus.Ok, null, null);

    public static Result<T> Fail<T>(string message, ResultStatus status = ResultStatus.Error) =>
        new(default!, false, status, message, null);

    public static Result<T> Fail<T>(IEnumerable<string> messages, ResultStatus status = ResultStatus.Invalid)
    {
        var list = messages.ToList();
        var primary = list.FirstOrDefault();
        return new Result<T>(default!, false, status, primary, list);
    }

    public static Result<T> NotFound<T>(string message) =>
        new(default!, false, ResultStatus.NotFound, message, null);

    public static Result<T> Invalid<T>(string message) =>
        new(default!, false, ResultStatus.Invalid, message, new[] { message });

    public static Result<T> Invalid<T>(IEnumerable<string> messages) =>
        Fail<T>(messages, ResultStatus.Invalid);

    public static Result<T> Conflict<T>(string message) =>
        new(default!, false, ResultStatus.Conflict, message, null);

    public static Result<T> Unauthorized<T>(string message) =>
        new(default!, false, ResultStatus.Unauthorized, message, null);

    public static Result<T> Forbidden<T>(string message) =>
        new(default!, false, ResultStatus.Forbidden, message, null);
}

public sealed class Result<T> : Result
{
    internal Result(T value, bool isSuccess, ResultStatus status, string? error, IReadOnlyList<string>? errors)
        : base(isSuccess, status, error, errors)
    {
        Value = value;
    }

    public T Value { get; }

    public static implicit operator Result<T>(T value) => Success(value);
}
