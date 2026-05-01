using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Ping;

public sealed class PingQueryHandler : IQueryHandler<PingQuery, Result<PingStatusDto>>
{
    public Task<Result<PingStatusDto>> HandleAsync(PingQuery query, CancellationToken cancellationToken = default)
    {
        var status = new PingStatusDto("healthy", DateTime.UtcNow);
        return Task.FromResult(Result.Ok<PingStatusDto>(status));
    }
}
