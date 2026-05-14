using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Positions;

public sealed class UpdatePositionCommandHandler
    : ICommandHandler<UpdatePositionCommand, Result<PositionResponse>>
{
    private readonly IPositionRepository _positionRepository;

    public UpdatePositionCommandHandler(IPositionRepository positionRepository)
        => _positionRepository = positionRepository;

    public async Task<Result<PositionResponse>> HandleAsync(UpdatePositionCommand cmd, CancellationToken ct = default)
    {
        var position = await _positionRepository.GetByIdAsync(cmd.PositionId, ct);
        if (position is null)
            return Result<PositionResponse>.Fail("POSITION_NOT_FOUND", $"Position '{cmd.PositionId}' was not found.");

        if (string.IsNullOrWhiteSpace(cmd.Code))
            return Result<PositionResponse>.Fail("INVALID_CODE", "Position code is required.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<PositionResponse>.Fail("INVALID_NAME", "Position name is required.");

        var normalizedCode = cmd.Code.Trim().ToUpperInvariant();
        if (!string.Equals(position.NormalizedCode, normalizedCode, StringComparison.Ordinal))
        {
            var exists = await _positionRepository.ExistsByNormalizedCodeAsync(normalizedCode, ct);
            if (exists)
                return Result<PositionResponse>.Fail("POSITION_ALREADY_EXISTS", $"Position code '{normalizedCode}' already exists.");
        }

        position.Update(cmd.Code, cmd.Name, cmd.Description);
        await _positionRepository.UpdateAsync(position, ct);
        await _positionRepository.SaveChangesAsync(ct);

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
