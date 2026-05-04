using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.MatchEvents;

public sealed class CreateMatchEventTypeCommandHandler
    : ICommandHandler<CreateMatchEventTypeCommand, Result<MatchEventTypeResponse>>
{
    private readonly IMatchEventTypeRepository _typeRepository;
    private readonly ITenantContext _tenantContext;

    public CreateMatchEventTypeCommandHandler(
        IMatchEventTypeRepository typeRepository,
        ITenantContext tenantContext)
    {
        _typeRepository = typeRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MatchEventTypeResponse>> HandleAsync(CreateMatchEventTypeCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Code))
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_INVALID_CODE", "Code is required.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_INVALID_NAME", "Name is required.");

        var normalizedCode = cmd.Code.Trim().ToUpperInvariant();
        var exists = await _typeRepository.ExistsByNormalizedCodeAsync(normalizedCode, null, ct);
        if (exists)
            return Result<MatchEventTypeResponse>.Fail("MATCH_EVENT_TYPE_ALREADY_EXISTS", "A match event type with the same code already exists.");

        var type = MatchEventType.Create(_tenantContext.TenantId, cmd.Code, cmd.Name, cmd.Points, cmd.IsSystemDefault);
        await _typeRepository.AddAsync(type, ct);
        await _typeRepository.SaveChangesAsync(ct);

        return Result<MatchEventTypeResponse>.Ok(ToResponse(type));
    }

    private static MatchEventTypeResponse ToResponse(MatchEventType type) => new(
        type.Id,
        type.TenantId,
        type.Code,
        type.Name,
        type.Points,
        type.IsSystemDefault,
        type.IsActive,
        type.CreatedAt);
}
