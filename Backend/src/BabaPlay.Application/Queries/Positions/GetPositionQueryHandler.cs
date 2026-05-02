using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Positions;

public sealed class GetPositionQueryHandler
    : IQueryHandler<GetPositionQuery, Result<PositionResponse>>
{
    private readonly IPositionRepository _positionRepository;

    public GetPositionQueryHandler(IPositionRepository positionRepository)
        => _positionRepository = positionRepository;

    public async Task<Result<PositionResponse>> HandleAsync(GetPositionQuery query, CancellationToken ct = default)
    {
        var position = await _positionRepository.GetByIdAsync(query.PositionId, ct);

        if (position is null || !position.IsActive)
            return Result<PositionResponse>.Fail("POSITION_NOT_FOUND", $"Position '{query.PositionId}' was not found.");

        return Result<PositionResponse>.Ok(new PositionResponse(
            position.Id,
            position.TenantId,
            position.Code,
            position.Name,
            position.Description,
            position.IsActive,
            position.CreatedAt));
    }
}
