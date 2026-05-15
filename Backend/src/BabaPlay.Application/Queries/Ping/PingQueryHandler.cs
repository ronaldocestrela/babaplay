using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Ping;

public sealed class PingQueryHandler : IQueryHandler<PingQuery, Result<PingStatusDto>>
{
    private readonly IApiReadinessProbe _readinessProbe;

    public PingQueryHandler(IApiReadinessProbe readinessProbe)
    {
        _readinessProbe = readinessProbe;
    }

    public async Task<Result<PingStatusDto>> HandleAsync(PingQuery query, CancellationToken cancellationToken = default)
    {
        var isMasterDatabaseReady = await _readinessProbe.IsMasterDatabaseReadyAsync(cancellationToken);
        var status = isMasterDatabaseReady ? "healthy" : "unhealthy";

        return Result.Ok(new PingStatusDto(status, DateTime.UtcNow));
    }
}
