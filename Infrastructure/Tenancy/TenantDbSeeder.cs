using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy;

public class TenantDbSeeder(TenantDbContext tenantDbContext, IServiceProvider serviceProvider) : ITenantDbSeeder
{
    private readonly TenantDbContext _tenantDbContext = tenantDbContext;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InitializeDatabaseAsync(CancellationToken ct)
    {
        await InitializeDatabaseWithTenantAsync(ct);

        foreach (var tenant in await _tenantDbContext.TenantInfo.ToListAsync(ct))
        {
            await InitializeApplicationDbForTenantAsync(tenant, ct);
        }
    }

    private async Task InitializeDatabaseWithTenantAsync(CancellationToken ct)
    {
        if (await _tenantDbContext.TenantInfo.FindAsync([TenancyConstants.Root.Id], ct) is null)
        {
            // Create tenant
            var rootTenant = new BabaPlayTenantInfo
            {
                Id = TenancyConstants.Root.Id,
                Identifier = TenancyConstants.Root.Id,
                Name = TenancyConstants.Root.Name,
                Email = TenancyConstants.Root.Email,
                FirstName = TenancyConstants.FirstName,
                LastName = TenancyConstants.LastName,
                IsActive = true,
                ValidUpTo = DateTime.UtcNow.AddYears(2)
            };

            await _tenantDbContext.TenantInfo.AddAsync(rootTenant, ct);
            await _tenantDbContext.SaveChangesAsync(ct);
        }
    }

    private async Task InitializeApplicationDbForTenantAsync(BabaPlayTenantInfo currentTenant, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<BabaPlayTenantInfo>
            {
                TenantInfo = currentTenant
            };

        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitializeDatabaseAsync(ct);
    }
}
