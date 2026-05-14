using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.MatchEvents;

public sealed class GetMatchEventTypesQueryHandler
    : IQueryHandler<GetMatchEventTypesQuery, Result<IReadOnlyList<MatchEventTypeResponse>>>
{
    private readonly IMatchEventTypeRepository _typeRepository;

    public GetMatchEventTypesQueryHandler(IMatchEventTypeRepository typeRepository)
        => _typeRepository = typeRepository;

    public async Task<Result<IReadOnlyList<MatchEventTypeResponse>>> HandleAsync(GetMatchEventTypesQuery query, CancellationToken ct = default)
    {
        var items = await _typeRepository.GetAllActiveAsync(ct);

        return Result<IReadOnlyList<MatchEventTypeResponse>>.Ok(items
            .Select(x => new MatchEventTypeResponse(
                x.Id,
                x.TenantId,
                x.Code,
                x.Name,
                x.Points,
                x.IsSystemDefault,
                x.IsActive,
                x.CreatedAt))
            .ToList());
    }
}
