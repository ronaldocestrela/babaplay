using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Positions;

public sealed class GetPositionsQueryHandler
    : IQueryHandler<GetPositionsQuery, Result<IReadOnlyList<PositionResponse>>>
{
    private readonly IPositionRepository _positionRepository;

    public GetPositionsQueryHandler(IPositionRepository positionRepository)
        => _positionRepository = positionRepository;

    public async Task<Result<IReadOnlyList<PositionResponse>>> HandleAsync(GetPositionsQuery query, CancellationToken ct = default)
    {
        var positions = await _positionRepository.GetAllActiveAsync(ct);

        return Result<IReadOnlyList<PositionResponse>>.Ok(
            positions.Select(p => new PositionResponse(
                p.Id,
                p.TenantId,
                p.Code,
                p.Name,
                p.Description,
                p.IsActive,
                p.CreatedAt)).ToList());
    }
}
