using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.Positions;

public sealed class CreatePositionCommandHandler
    : ICommandHandler<CreatePositionCommand, Result<PositionResponse>>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITenantContext _tenantContext;

    public CreatePositionCommandHandler(IPositionRepository positionRepository, ITenantContext tenantContext)
    {
        _positionRepository = positionRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PositionResponse>> HandleAsync(CreatePositionCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Code))
            return Result<PositionResponse>.Fail("INVALID_CODE", "Position code is required.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<PositionResponse>.Fail("INVALID_NAME", "Position name is required.");

        var normalizedCode = cmd.Code.Trim().ToUpperInvariant();
        var exists = await _positionRepository.ExistsByNormalizedCodeAsync(normalizedCode, ct);
        if (exists)
            return Result<PositionResponse>.Fail("POSITION_ALREADY_EXISTS", $"Position code '{normalizedCode}' already exists.");

        var position = Position.Create(_tenantContext.TenantId, cmd.Code, cmd.Name, cmd.Description);

        await _positionRepository.AddAsync(position, ct);
        await _positionRepository.SaveChangesAsync(ct);

        return Result<PositionResponse>.Ok(ToResponse(position));
    }

    private static PositionResponse ToResponse(Position position) => new(
        position.Id,
        position.TenantId,
        position.Code,
        position.Name,
        position.Description,
        position.IsActive,
        position.CreatedAt);
}
