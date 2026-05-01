namespace BabaPlay.Domain.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, Guid id)
        : base("NOT_FOUND", $"{entityName} with id '{id}' was not found.")
    {
    }
}
