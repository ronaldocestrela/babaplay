namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Handles a write command and returns a typed result.
/// CQRS principle: handlers are NEVER shared between commands and queries.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
