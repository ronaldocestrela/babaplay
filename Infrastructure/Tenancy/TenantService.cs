using Application.Features.Tenancy;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Contexts;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tenancy;

public class TenantService(IMultiTenantStore<BabaPlayTenantInfo> tenantStore, ApplicationDbSeeder dbSeeder, IServiceProvider serviceProvider) : ITenantService
{
    private readonly IMultiTenantStore<BabaPlayTenantInfo> _tenantStore = tenantStore;
    private readonly ApplicationDbSeeder _dbSeeder = dbSeeder;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<string> ActivateAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);
        tenantInDb.IsActive = true;

        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
    }

    public async Task<string> CreateTenantAsync(CreateTenantRequest createTenant, CancellationToken ct)
    {
        var newTenant = new BabaPlayTenantInfo
        {
            Id = createTenant.Identifier,
            Identifier = createTenant.Identifier,
            Name = createTenant.Name,
            IsActive = createTenant.IsActive,
            ConnectionString = createTenant.ConnectionString,
            Email = createTenant.Email,
            FirstName = createTenant.FirstName,
            LastName = createTenant.LastName,
            ValidUpTo = createTenant.ValidUpTo
        };

        await _tenantStore.TryAddAsync(newTenant);

        // Seeding tenant data
        using var scope = _serviceProvider.CreateScope();

        _serviceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<BabaPlayTenantInfo>();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbSeeder>()
            .InitializeDatabaseAsync(ct);

        return newTenant.Identifier;
    }

    public async Task<string> DeactivateAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);
        tenantInDb.IsActive = false;

        await _tenantStore.TryUpdateAsync(tenantInDb);
        return tenantInDb.Identifier;
    }

    public async Task<TenantResponse> GetTenantByIdAsync(string id)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(id);

        #region Manual Mapping
        //var tenantResponse = new TenantResponse
        //{
        //    Identifier = tenantInDb.Identifier,
        //    Name = tenantInDb.Name,
        //    ConnectionString = tenantInDb.ConnectionString,
        //    Email = tenantInDb.Email,
        //    FirstName = tenantInDb.FirstName,
        //    LastName = tenantInDb.LastName,
        //    IsActive = tenantInDb.IsActive,
        //    ValidUpTo = tenantInDb.ValidUpTo
        //};
        //return tenantResponse;
        #endregion
        // Mapster
        return tenantInDb.Adapt<TenantResponse>();

    }

    public async Task<List<TenantResponse>> GetTenantsAsync()
    {
        var tenantsInDb = await _tenantStore.GetAllAsync();
        return tenantsInDb.Adapt<List<TenantResponse>>();
    }

    public async Task<string> UpdateSubscriptionAsync(UpdateTenantSubscriptionRequest updateTenantSubscription)
    {
        var tenantInDb = await _tenantStore.TryGetAsync(updateTenantSubscription.TenantId);

        tenantInDb.ValidUpTo = updateTenantSubscription.NewExpiryDate;

        await _tenantStore.TryUpdateAsync(tenantInDb);

        return tenantInDb.Identifier;
    }
}
