using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BabaPlay.Infrastructure.Persistence;

public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var cfg = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine(basePath, "BabaPlay.Api", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine("src", "BabaPlay.Api", "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg["Database:PlatformConnectionString"]
                 ?? throw new InvalidOperationException("Set Database:PlatformConnectionString");

        var ob = new DbContextOptionsBuilder<PlatformDbContext>();
        ob.UseSqlServer(cs);
        return new PlatformDbContext(ob.Options);
    }
}
