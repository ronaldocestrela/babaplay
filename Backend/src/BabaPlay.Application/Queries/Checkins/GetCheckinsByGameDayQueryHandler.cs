using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Checkins;

public sealed class GetCheckinsByGameDayQueryHandler
    : IQueryHandler<GetCheckinsByGameDayQuery, Result<IReadOnlyList<CheckinResponse>>>
{
    private readonly ICheckinRepository _checkinRepository;

    public GetCheckinsByGameDayQueryHandler(ICheckinRepository checkinRepository)
    {
        _checkinRepository = checkinRepository;
    }

    public async Task<Result<IReadOnlyList<CheckinResponse>>> HandleAsync(GetCheckinsByGameDayQuery query, CancellationToken cancellationToken = default)
    {
        var checkins = await _checkinRepository.GetActiveByGameDayAsync(query.GameDayId, cancellationToken);

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
