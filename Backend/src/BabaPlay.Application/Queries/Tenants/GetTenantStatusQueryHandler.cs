using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Domain.Exceptions;

namespace BabaPlay.Application.Queries.Tenants;

/// <summary>Handles provisioning status queries for a single tenant.</summary>
public sealed class GetTenantStatusQueryHandler
    : IQueryHandler<GetTenantStatusQuery, Result<TenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTenantStatusQueryHandler(ITenantRepository tenantRepository)
        => _tenantRepository = tenantRepository;

    /// <inheritdoc />
    public async Task<Result<TenantResponse>> HandleAsync(
        GetTenantStatusQuery query,
        CancellationToken ct = default)
    {
        var tenant = await _tenantRepository.GetByIdAsync(query.TenantId, ct);

        if (tenant is null)
            throw new NotFoundException("TENANT_NOT_FOUND", $"Tenant '{query.TenantId}' was not found.");

        return Result<TenantResponse>.Ok(new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.ProvisioningStatus));
    }
}
