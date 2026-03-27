using BabaPlay.Modules.Platform.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BabaPlay.Modules.Platform.Services;

public sealed class TenantSubscriptionService
{
    private readonly IPlatformRepository<Tenant> _tenants;
    private readonly IPlatformRepository<Subscription> _subscriptions;
    private readonly IPlatformRepository<Plan> _plans;
    private readonly IPlatformUnitOfWork _uow;
    private readonly ITenantProvisioningService _provisioning;
    private readonly string _platformConnectionString;

    public TenantSubscriptionService(
        IPlatformRepository<Tenant> tenants,
        IPlatformRepository<Subscription> subscriptions,
        IPlatformRepository<Plan> plans,
        IPlatformUnitOfWork uow,
        ITenantProvisioningService provisioning,
        IConfiguration configuration)
    {
        _tenants = tenants;
        _subscriptions = subscriptions;
        _plans = plans;
        _uow = uow;
        _provisioning = provisioning;
        _platformConnectionString = configuration["Database:PlatformConnectionString"]
                                    ?? throw new InvalidOperationException("Database:PlatformConnectionString is required.");
    }

    public async Task<Result<IReadOnlyList<Tenant>>> ListTenantsAsync(CancellationToken ct)
    {
        var list = await _tenants.Query().OrderBy(t => t.Name).ToListAsync(ct);
        return Result.Success<IReadOnlyList<Tenant>>(list);
    }

    public async Task<Result<Tenant>> GetTenantAsync(string id, CancellationToken ct)
    {
        var t = await _tenants.GetByIdAsync(id, ct);
        return t is null ? Result.NotFound<Tenant>("Tenant not found.") : Result.Success(t);
    }

    public async Task<Result<Tenant>> CreateTenantAsync(string name, string subdomain, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Tenant>("Name is required.");
        if (string.IsNullOrWhiteSpace(subdomain)) return Result.Invalid<Tenant>("Subdomain is required.");
        subdomain = subdomain.Trim().ToLowerInvariant();
        if (await _tenants.Query().AnyAsync(x => x.Subdomain == subdomain, ct))
            return Result.Conflict<Tenant>("Subdomain already in use.");

        var tenant = new Tenant
        {
            Name = name.Trim(),
            Subdomain = subdomain,
            DatabaseName = $"BabaPlay_{Guid.NewGuid():N}"
        };

        await _tenants.AddAsync(tenant, ct);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(tenant);
    }

    public async Task<Result<Tenant>> UpdateTenantAsync(string id, string name, string subdomain, CancellationToken ct)
    {
        var tenant = await _tenants.GetByIdAsync(id, ct);
        if (tenant is null) return Result.NotFound<Tenant>("Tenant not found.");
        if (string.IsNullOrWhiteSpace(name)) return Result.Invalid<Tenant>("Name is required.");
        if (string.IsNullOrWhiteSpace(subdomain)) return Result.Invalid<Tenant>("Subdomain is required.");
        subdomain = subdomain.Trim().ToLowerInvariant();
        if (await _tenants.Query().AnyAsync(x => x.Subdomain == subdomain && x.Id != id, ct))
            return Result.Conflict<Tenant>("Subdomain already in use.");

        tenant.Name = name.Trim();
        tenant.Subdomain = subdomain;
        tenant.UpdatedAt = DateTime.UtcNow;
        _tenants.Update(tenant);
        await _uow.SaveChangesAsync(ct);
        return Result.Success(tenant);
    }

    public async Task<Result> DeleteTenantAsync(string id, CancellationToken ct)
    {
        var tenant = await _tenants.GetByIdAsync(id, ct);
        if (tenant is null) return Result.Failure("Tenant not found.", ResultStatus.NotFound);
        _tenants.Remove(tenant);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<Subscription>> SubscribeTenantAsync(string tenantId, string planId, CancellationToken ct)
    {
        var tenant = await _tenants.GetByIdAsync(tenantId, ct);
        if (tenant is null) return Result.NotFound<Subscription>("Tenant not found.");
        var plan = await _plans.GetByIdAsync(planId, ct);
        if (plan is null) return Result.NotFound<Subscription>("Plan not found.");

        var sub = new Subscription
        {
            TenantId = tenantId,
            PlanId = planId,
            StartDate = DateTime.UtcNow,
            Status = SubscriptionStatus.Active
        };
        await _subscriptions.AddAsync(sub, ct);
        await _uow.SaveChangesAsync(ct);

        var provision = await _provisioning.ProvisionDatabaseAsync(tenant.DatabaseName, _platformConnectionString, ct);
        if (provision.IsFailure)
            return Result.Fail<Subscription>(provision.Error ?? "Provisioning failed.");

        return Result.Success(sub);
    }
}
