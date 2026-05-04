using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed class UpdateMatchEventTypeCommandHandler
    : ICommandHandler<UpdateMatchEventTypeCommand, Result<MatchEventTypeResponse>>
{
    private readonly IMatchEventTypeRepository _typeRepository;

    public UpdateMatchEventTypeCommandHandler(IMatchEventTypeRepository typeRepository)
        => _typeRepository = typeRepository;

    public async Task<Result<MatchEventTypeResponse>> HandleAsync(UpdateMatchEventTypeCommand cmd, CancellationToken ct = default)
    {
        var type = await _typeRepository.GetByIdAsync(cmd.MatchEventTypeId, ct);
        if (type is null)
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_NOT_FOUND", "Match event type was not found.");

        if (string.IsNullOrWhiteSpace(cmd.Code))
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_INVALID_CODE", "Code is required.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_INVALID_NAME", "Name is required.");

        var normalizedCode = cmd.Code.Trim().ToUpperInvariant();
        var exists = await _typeRepository.ExistsByNormalizedCodeAsync(normalizedCode, cmd.MatchEventTypeId, ct);
        if (exists)
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_ALREADY_EXISTS", "A match event type with the same code already exists.");

        type.Update(cmd.Code, cmd.Name, cmd.Points);
        await _typeRepository.UpdateAsync(type, ct);
        await _typeRepository.SaveChangesAsync(ct);

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
