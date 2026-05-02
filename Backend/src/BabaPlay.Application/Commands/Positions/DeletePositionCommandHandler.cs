using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Positions;

public sealed class DeletePositionCommandHandler
    : ICommandHandler<DeletePositionCommand, Result>
{
    private readonly IPositionRepository _positionRepository;

    public DeletePositionCommandHandler(IPositionRepository positionRepository)
        => _positionRepository = positionRepository;

    public async Task<Result> HandleAsync(DeletePositionCommand cmd, CancellationToken ct = default)
    {
        var position = await _positionRepository.GetByIdAsync(cmd.PositionId, ct);
        if (position is null)
            return Result.Fail("POSITION_NOT_FOUND", $"Position '{cmd.PositionId}' was not found.");

        var isInUse = await _positionRepository.IsInUseAsync(cmd.PositionId, ct);
        if (isInUse)
            return Result.Fail("POSITION_IN_USE", "Position cannot be deleted because it is assigned to one or more players.");

        position.Deactivate();
        await _positionRepository.UpdateAsync(position, ct);
        await _positionRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
