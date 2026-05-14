using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Tenants;

public sealed record GetTenantSettingsQuery(Guid TenantId)
    : IQuery<Result<TenantResponse>>;
