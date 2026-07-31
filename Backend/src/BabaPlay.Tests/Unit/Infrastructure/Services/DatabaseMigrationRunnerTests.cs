using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public sealed class DatabaseMigrationRunnerTests
{
    [Fact]
    public async Task RunAsync_ShouldFail_WhenAppDbUsesNonRelationalProvider()
    {
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        await using var appDb = new AppDbContext(appOptions);
        var sut = new DatabaseMigrationRunner(appDb);

        var act = async () => await sut.RunAsync();

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*relational provider*");
    }

    [Fact]
    public void AddInfrastructureServices_ShouldRegisterDatabaseMigrationRunner()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:MasterDb"] = "Server=localhost;Database=BabaPlay_Master;User Id=sa;Password=Test@123;TrustServerCertificate=true;",
                ["Jwt:SecretKey"] = "test-secret-key-at-least-256-bits-long-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                ["Jwt:Issuer"] = "BabaPlay.Api",
                ["Jwt:Audience"] = "BabaPlay.Clients",
            })
            .Build();

        services.AddInfrastructureServices(configuration);

        services.Should().Contain(d => d.ServiceType == typeof(DatabaseMigrationRunner));
        services.Should().Contain(d => d.ServiceType == typeof(ITenantRbacCatalogSyncService));
    }
}