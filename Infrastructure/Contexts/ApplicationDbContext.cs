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
}
