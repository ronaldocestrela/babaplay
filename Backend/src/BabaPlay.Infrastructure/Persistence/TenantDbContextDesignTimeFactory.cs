using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BabaPlay.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used exclusively by EF Core tooling (migrations).
/// Uses a placeholder SQL Server connection string — never executed at runtime.
/// </summary>
public sealed class TenantDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlServer("Server=.;Database=BabaPlay_TenantDesignTime;Trusted_Connection=True;")
            .Options;

        return new TenantDbContext(options);
    }
}
