using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Checkins;

public sealed class GetCheckinsByPlayerQueryHandler
    : IQueryHandler<GetCheckinsByPlayerQuery, Result<IReadOnlyList<CheckinResponse>>>
{
    private readonly ICheckinRepository _checkinRepository;

    public GetCheckinsByPlayerQueryHandler(ICheckinRepository checkinRepository)
    {
        _checkinRepository = checkinRepository;
    }

    public async Task<Result<IReadOnlyList<CheckinResponse>>> HandleAsync(GetCheckinsByPlayerQuery query, CancellationToken cancellationToken = default)
    {
        var checkins = await _checkinRepository.GetActiveByPlayerAsync(query.PlayerId, cancellationToken);

        var mapped = checkins.Select(checkin => new CheckinResponse(
                checkin.Id,
                checkin.TenantId,
                checkin.PlayerId,
                checkin.GameDayId,
                checkin.CheckedInAtUtc,
                checkin.Latitude,
                checkin.Longitude,
                checkin.DistanceFromAssociationMeters,
                checkin.IsActive,
                checkin.CreatedAt,
                checkin.CancelledAtUtc))
            .ToList();

        return Result<IReadOnlyList<CheckinResponse>>.Ok(mapped);
    }
}
