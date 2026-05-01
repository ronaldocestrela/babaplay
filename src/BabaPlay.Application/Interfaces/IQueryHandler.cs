namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Handles a read query and returns a typed result.
/// CQRS principle: handlers are NEVER shared between commands and queries.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
