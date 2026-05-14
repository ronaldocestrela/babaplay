using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Entities;

namespace BabaPlay.Application.Commands.TenantGameDayOptions;

public sealed class CreateTenantGameDayOptionCommandHandler
    : ICommandHandler<CreateTenantGameDayOptionCommand, Result<TenantGameDayOptionResponse>>
{
    private readonly ITenantGameDayOptionRepository _repository;
    private readonly IUserTenantRepository _userTenantRepository;

    public CreateTenantGameDayOptionCommandHandler(
        ITenantGameDayOptionRepository repository,
        IUserTenantRepository userTenantRepository)
    {
        _repository = repository;
        _userTenantRepository = userTenantRepository;
    }

    public async Task<Result<TenantGameDayOptionResponse>> HandleAsync(CreateTenantGameDayOptionCommand cmd, CancellationToken ct = default)
    {
        if (cmd.TenantId == Guid.Empty)
            return Result<TenantGameDayOptionResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        if (string.IsNullOrWhiteSpace(cmd.RequestedByUserId))
            return Result<TenantGameDayOptionResponse>.Fail("UNAUTHORIZED", "Authenticated user is required.");

        var isOwner = await _userTenantRepository.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, ct);
        if (!isOwner)
            return Result<TenantGameDayOptionResponse>.Fail("FORBIDDEN", "Only tenant admins can manage game day options.");

        var exists = await _repository.ExistsActiveBySlotAsync(cmd.TenantId, cmd.DayOfWeek, cmd.LocalStartTime, null, ct);
        if (exists)
            return Result<TenantGameDayOptionResponse>.Fail("TENANT_GAMEDAY_OPTION_ALREADY_EXISTS", "An active option with the same day and time already exists.");

        var option = TenantGameDayOption.Create(cmd.TenantId, cmd.DayOfWeek, cmd.LocalStartTime);
        await _repository.AddAsync(option, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<TenantGameDayOptionResponse>.Ok(ToResponse(option));
    }

    private static TenantGameDayOptionResponse ToResponse(TenantGameDayOption option)
        => new(option.Id, option.TenantId, option.DayOfWeek, option.LocalStartTime, option.IsActive, option.CreatedAt, option.UpdatedAt);
}
