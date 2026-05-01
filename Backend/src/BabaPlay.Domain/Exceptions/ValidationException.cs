namespace BabaPlay.Domain.Exceptions;

public class ValidationException : DomainException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string field, string error)
        : this(new Dictionary<string, string[]> { { field, [error] } })
    {
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}
