using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Ping;

public sealed class PingCommandHandler : ICommandHandler<PingCommand, Result<string>>
{
    public Task<Result<string>> HandleAsync(PingCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Sender))
            return Task.FromResult(Result.Fail<string>("VALIDATION_ERROR", "Sender cannot be empty."));

        return Task.FromResult(Result.Ok<string>($"pong from BabaPlay to '{command.Sender}'"));
    }
}
