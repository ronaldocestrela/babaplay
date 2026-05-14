using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.TenantGameDayOptions;

public sealed record GetTenantGameDayOptionsQuery(Guid TenantId, bool? IsActive)
    : IQuery<Result<IReadOnlyList<TenantGameDayOptionResponse>>>;
