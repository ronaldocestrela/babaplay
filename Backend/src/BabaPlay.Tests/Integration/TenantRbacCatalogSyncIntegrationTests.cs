using System.Net;
using BabaPlay.Application.Common;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Integration;

public sealed class TenantRbacCatalogSyncIntegrationTests : IClassFixture<RbacWebApplicationFactory>
{
    private readonly RbacWebApplicationFactory _factory;

    public TenantRbacCatalogSyncIntegrationTests(RbacWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SyncTenantAsync_WhenAdminMissingFinancialRead_ShouldAllowFinancialOverview()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sync = scope.ServiceProvider.GetRequiredService<ITenantRbacCatalogSyncService>();

        var adminRole = await db.Roles
            .IgnoreQueryFilters()
            .Include(r => r.Permissions)
            .SingleAsync(r =>
                r.TenantId == RbacWebApplicationFactory.TenantAId &&
                r.NormalizedName == RbacCatalog.Roles.Admin.ToUpperInvariant());

        var financialPermissions = await db.Permissions
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == RbacWebApplicationFactory.TenantAId &&
                        (p.NormalizedCode == RbacCatalog.Permissions.FinancialRead.ToUpperInvariant() ||
                         p.NormalizedCode == RbacCatalog.Permissions.FinancialWrite.ToUpperInvariant() ||
                         p.NormalizedCode == RbacCatalog.Permissions.FinancialApprove.ToUpperInvariant()))
            .ToListAsync();

        foreach (var permission in financialPermissions)
        {
            adminRole.RemovePermission(permission.Id);
        }

        await db.SaveChangesAsync();

        using var requestBeforeSync = new HttpRequestMessage(HttpMethod.Get, "/api/v1/financial/overview");
        requestBeforeSync.Headers.Authorization = new("Bearer", "test-token");
        requestBeforeSync.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        requestBeforeSync.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        using var client = _factory.CreateClient();
        var beforeSync = await client.SendAsync(requestBeforeSync);
        beforeSync.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var syncResult = await sync.SyncTenantAsync(RbacWebApplicationFactory.TenantAId);
        syncResult.IsSuccess.Should().BeTrue();

        using var requestAfterSync = new HttpRequestMessage(HttpMethod.Get, "/api/v1/financial/overview");
        requestAfterSync.Headers.Authorization = new("Bearer", "test-token");
        requestAfterSync.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        requestAfterSync.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var afterSync = await client.SendAsync(requestAfterSync);
        afterSync.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
