using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class TenantGeolocationSettingsRepository : ITenantGeolocationSettingsRepository
{
    private readonly MasterDbContext _masterDbContext;

    public TenantGeolocationSettingsRepository(MasterDbContext masterDbContext)
    {
        _masterDbContext = masterDbContext;
    }

    public async Task<TenantGeolocationSettingsDto?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _masterDbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenantId && t.IsActive)
            .Select(t => new
            {
                t.AssociationLatitude,
                t.AssociationLongitude,
                t.CheckinRadiusMeters,
            })
            .FirstOrDefaultAsync(ct);

        if (tenant is null
            || !tenant.AssociationLatitude.HasValue
            || !tenant.AssociationLongitude.HasValue
            || !tenant.CheckinRadiusMeters.HasValue)
        {
            return null;
        }

        return new TenantGeolocationSettingsDto(
            tenant.AssociationLatitude.Value,
            tenant.AssociationLongitude.Value,
            tenant.CheckinRadiusMeters.Value);
    }
}
