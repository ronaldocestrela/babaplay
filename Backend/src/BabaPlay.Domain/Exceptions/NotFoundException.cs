namespace BabaPlay.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, Guid id)
        : base("NOT_FOUND", $"{entityName} with id '{id}' was not found.")
    {
    }

    /// <summary>Constructor for custom error codes (e.g. TENANT_NOT_FOUND).</summary>
    public NotFoundException(string code, string message)
        : base(code, message)
    {
    }
}
