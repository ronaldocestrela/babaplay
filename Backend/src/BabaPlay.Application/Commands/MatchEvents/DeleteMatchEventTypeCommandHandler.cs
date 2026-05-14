using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed class DeleteMatchEventTypeCommandHandler
    : ICommandHandler<DeleteMatchEventTypeCommand, Result>
{
    private readonly IMatchEventTypeRepository _typeRepository;

    public DeleteMatchEventTypeCommandHandler(IMatchEventTypeRepository typeRepository)
        => _typeRepository = typeRepository;

    public async Task<Result> HandleAsync(DeleteMatchEventTypeCommand cmd, CancellationToken ct = default)
    {
        var type = await _typeRepository.GetByIdAsync(cmd.MatchEventTypeId, ct);
        if (type is null)
            return Result.Fail("MATCH_EVENT_TYPE_NOT_FOUND", "Match event type was not found.");

        type.Deactivate();
        await _typeRepository.UpdateAsync(type, ct);
        await _typeRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
