using BabaPlay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class TenantMigrationOrchestratorTests
{
    private const string PlatformCs = "Server=.;Database=Platform;Trusted_Connection=True;TrustServerCertificate=True";

    [Fact]
    public async Task MigrateAllAsync_EmptyPlatformConnection_ReturnsZerosWithoutCallingMigrator()
    {
        var migrator = new Mock<ITenantDatabaseMigrator>(MockBehavior.Strict);
        var sut = new TenantMigrationOrchestrator(migrator.Object, NullLogger<TenantMigrationOrchestrator>.Instance);

        var result = await sut.MigrateAllAsync(
            new List<(string Subdomain, string DatabaseName)> { ("t1", "BabaPlay_1") },
            string.Empty,
            CancellationToken.None);

        result.Should().BeEquivalentTo(new TenantMigrationBatchResult(0, 0, 0, 0));
        migrator.Verify(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MigrateAllAsync_AllSucceed_MigratedEqualsCount()
    {
        var migrator = new Mock<ITenantDatabaseMigrator>();
        migrator
            .Setup(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new TenantMigrationOrchestrator(migrator.Object, NullLogger<TenantMigrationOrchestrator>.Instance);

        var tenants = new List<(string Subdomain, string DatabaseName)>
        {
            ("a", "DbA"),
            ("b", "DbB")
        };

        var result = await sut.MigrateAllAsync(tenants, PlatformCs, CancellationToken.None);

        result.Should().BeEquivalentTo(new TenantMigrationBatchResult(2, 2, 0, 0));
        migrator.Verify(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task MigrateAllAsync_OneFails_ContinuesAndCountsFailure()
    {
        var migrator = new Mock<ITenantDatabaseMigrator>();
        var call = 0;
        migrator
            .Setup(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                call++;
                if (call == 2)
                    throw new InvalidOperationException("boom");
                return Task.CompletedTask;
            });

        var sut = new TenantMigrationOrchestrator(migrator.Object, NullLogger<TenantMigrationOrchestrator>.Instance);

        var tenants = new List<(string Subdomain, string DatabaseName)>
        {
            ("a", "DbA"),
            ("b", "DbB"),
            ("c", "DbC")
        };

        var result = await sut.MigrateAllAsync(tenants, PlatformCs, CancellationToken.None);

        result.Should().BeEquivalentTo(new TenantMigrationBatchResult(3, 2, 1, 0));
        migrator.Verify(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task MigrateAllAsync_EmptyDatabaseName_SkipsWithoutCallingMigratorForThatRow()
    {
        var migrator = new Mock<ITenantDatabaseMigrator>();
        migrator
            .Setup(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new TenantMigrationOrchestrator(migrator.Object, NullLogger<TenantMigrationOrchestrator>.Instance);

        var tenants = new List<(string Subdomain, string DatabaseName)>
        {
            ("ok", "DbOk"),
            ("bad", "  "),
            ("also", "")
        };

        var result = await sut.MigrateAllAsync(tenants, PlatformCs, CancellationToken.None);

        result.Should().BeEquivalentTo(new TenantMigrationBatchResult(3, 1, 0, 2));
        migrator.Verify(m => m.MigrateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
