using BabaPlay.Application.Common;
using BabaPlay.Domain.Entities;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using BabaPlay.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public sealed class TenantRbacCatalogSyncServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly TenantRbacCatalogSyncService _sut;
    private readonly Guid _tenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    public TenantRbacCatalogSyncServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _db.Tenants.Add(new Tenant
        {
            Id = _tenantId,
            Name = "Legacy Tenant",
            Slug = "legacy-tenant",
            IsActive = true,
        });
        _db.SaveChanges();

        _sut = new TenantRbacCatalogSyncService(_db);
    }

    [Fact]
    public async Task SyncTenantAsync_WhenAdminMissingFinancialPermissions_ShouldGrantCatalogPermissions()
    {
        var adminRole = Role.Create(_tenantId, RbacCatalog.Roles.Admin, "Legacy admin");
        var rolesRead = Permission.Create(_tenantId, RbacCatalog.Permissions.RbacRolesRead, "Read roles");
        adminRole.AddPermission(rolesRead.Id);

        _db.Roles.Add(adminRole);
        _db.Permissions.Add(rolesRead);
        await _db.SaveChangesAsync();

        var result = await _sut.SyncTenantAsync(_tenantId);

        result.IsSuccess.Should().BeTrue();

        var financialReadExists = await _db.Permissions
            .IgnoreQueryFilters()
            .AnyAsync(p => p.TenantId == _tenantId &&
                           p.NormalizedCode == RbacCatalog.Permissions.FinancialRead.ToUpperInvariant());

        financialReadExists.Should().BeTrue();

        var adminHasFinancialRead = await (
            from role in _db.Roles
            join rolePermission in _db.RolePermissions on role.Id equals rolePermission.RoleId
            join permission in _db.Permissions on rolePermission.PermissionId equals permission.Id
            where role.TenantId == _tenantId
                  && role.NormalizedName == RbacCatalog.Roles.Admin.ToUpperInvariant()
                  && permission.NormalizedCode == RbacCatalog.Permissions.FinancialRead.ToUpperInvariant()
            select permission.Id
        ).AnyAsync();

        adminHasFinancialRead.Should().BeTrue();
    }

    [Fact]
    public async Task SyncTenantAsync_WhenRunTwice_ShouldNotDuplicateRolePermissions()
    {
        var first = await _sut.SyncTenantAsync(_tenantId);
        var second = await _sut.SyncTenantAsync(_tenantId);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();

        var adminRoleId = await _db.Roles
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == _tenantId && r.NormalizedName == RbacCatalog.Roles.Admin.ToUpperInvariant())
            .Select(r => r.Id)
            .SingleAsync();

        var permissionCount = await _db.RolePermissions
            .CountAsync(rp => rp.RoleId == adminRoleId);

        permissionCount.Should().Be(RbacCatalog.DefaultRolePermissions[RbacCatalog.Roles.Admin].Count);
    }

    [Fact]
    public async Task SyncAllTenantsAsync_ShouldSyncOnlyActiveTenants()
    {
        var inactiveTenantId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        _db.Tenants.Add(new Tenant
        {
            Id = inactiveTenantId,
            Name = "Inactive Tenant",
            Slug = "inactive-tenant",
            IsActive = false,
        });
        await _db.SaveChangesAsync();

        var result = await _sut.SyncAllTenantsAsync();

        result.IsSuccess.Should().BeTrue();

        var activePermissions = await _db.Permissions
            .IgnoreQueryFilters()
            .CountAsync(p => p.TenantId == _tenantId);

        var inactivePermissions = await _db.Permissions
            .IgnoreQueryFilters()
            .CountAsync(p => p.TenantId == inactiveTenantId);

        activePermissions.Should().BeGreaterThan(0);
        inactivePermissions.Should().Be(0);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
