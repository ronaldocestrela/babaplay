using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Queries.Tenants;

public sealed class GetTenantSettingsQueryHandler
    : IQueryHandler<GetTenantSettingsQuery, Result<TenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantSettingsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<TenantResponse>> HandleAsync(GetTenantSettingsQuery query, CancellationToken ct = default)
    {
        if (query.TenantId == Guid.Empty)
            throw new NotFoundException("TENANT_NOT_RESOLVED", "Tenant context is required.");

        var tenant = await _tenantRepository.GetByIdAsync(query.TenantId, ct);

        if (tenant is null)
            throw new NotFoundException("TENANT_NOT_FOUND", $"Tenant '{query.TenantId}' was not found.");

        return Result<TenantResponse>.Ok(new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.ProvisioningStatus,
            tenant.PlayersPerTeam,
            tenant.LogoPath,
            tenant.Street,
            tenant.Number,
            tenant.Neighborhood,
            tenant.City,
            tenant.State,
            tenant.ZipCode));
    }
}
