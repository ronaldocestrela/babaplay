using Domain.Entities;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Contexts;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(
            IMultiTenantContextAccessor<BabaPlayTenantInfo> tenantInfoContextAccessor,
            DbContextOptions<ApplicationDbContext> options)
            : base(tenantInfoContextAccessor, options)
    {
    }

    public DbSet<Association> Associations => Set<Association>();
    public DbSet<Associado> Associados => Set<Associado>();

    // NOTE: CorsOrigin is intentionally *not* exposed here.  the CORS table
    // lives in the shared database and is accessed through SharedDbContext
    // so that all tenants share a single list.  the mapping configuration is
    // skipped in BaseDbContext to prevent the table from being created in
    // tenant databases.
}
