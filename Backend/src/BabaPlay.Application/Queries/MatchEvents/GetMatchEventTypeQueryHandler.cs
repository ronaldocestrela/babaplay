using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed class GetMatchEventTypeQueryHandler
    : IQueryHandler<GetMatchEventTypeQuery, Result<MatchEventTypeResponse>>
{
    private readonly IMatchEventTypeRepository _typeRepository;

    public GetMatchEventTypeQueryHandler(IMatchEventTypeRepository typeRepository)
        => _typeRepository = typeRepository;

    public async Task<Result<MatchEventTypeResponse>> HandleAsync(GetMatchEventTypeQuery query, CancellationToken ct = default)
    {
        var type = await _typeRepository.GetByIdAsync(query.MatchEventTypeId, ct);
        if (type is null)
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_NOT_FOUND", "Match event type was not found.");

        return Result<MatchEventTypeResponse>.Ok(new MatchEventTypeResponse(
            type.Id,
            type.TenantId,
            type.Code,
            type.Name,
            type.Points,
            type.IsSystemDefault,
            type.IsActive,
            type.CreatedAt));
    }
}
