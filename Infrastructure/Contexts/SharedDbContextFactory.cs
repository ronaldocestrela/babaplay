using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Contexts;

// used by EF tooling to create a context when running migrations
public class SharedDbContextFactory : IDesignTimeDbContextFactory<SharedDbContext>
{
    public SharedDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SharedDbContext>();
        // hard-coded connection; adjust if you use environment-specific settings
        optionsBuilder.UseSqlServer("Server=localhost,3713;Database=BabaPlaySharedDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True");
        return new SharedDbContext(optionsBuilder.Options);
    }
}
