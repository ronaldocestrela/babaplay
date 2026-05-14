using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.TenantGameDayOptions;

public sealed record CreateTenantGameDayOptionCommand(
    Guid TenantId,
    string RequestedByUserId,
    DayOfWeek DayOfWeek,
    TimeOnly LocalStartTime)
    : ICommand<Result<TenantGameDayOptionResponse>>;
