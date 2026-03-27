using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine(basePath, "BabaPlay.Api", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine("src", "BabaPlay.Api", "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg["Database:TenantTemplateConnectionString"]
                 ?? cfg["Database:PlatformConnectionString"]
                 ?? throw new InvalidOperationException("Set Database:TenantTemplateConnectionString or PlatformConnectionString for migrations.");

        var ob = new DbContextOptionsBuilder<TenantDbContext>();
        ob.UseSqlServer(cs);
        return new TenantDbContext(ob.Options);
    }
}
