using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Tenants;

/// <summary>Returns the current provisioning status for a tenant.</summary>
public sealed record GetTenantStatusQuery(Guid TenantId) : IQuery<Result<TenantResponse>>;
