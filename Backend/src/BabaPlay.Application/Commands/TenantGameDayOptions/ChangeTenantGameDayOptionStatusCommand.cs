using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.TenantGameDayOptions;

public sealed record ChangeTenantGameDayOptionStatusCommand(
    Guid TenantId,
    Guid OptionId,
    string RequestedByUserId,
    bool IsActive)
    : ICommand<Result<TenantGameDayOptionResponse>>;
