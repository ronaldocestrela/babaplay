using BabaPlay.Modules.Platform.Entities;
using BabaPlay.Modules.Platform.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Services;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class TenantSubscriptionServiceTests
{
    private readonly Mock<IPlatformRepository<Tenant>> _tenantRepo;
    private readonly Mock<IPlatformRepository<Subscription>> _subRepo;
    private readonly Mock<IPlatformRepository<Plan>> _planRepo;
    private readonly Mock<IPlatformUnitOfWork> _uow;
    private readonly Mock<ITenantProvisioningService> _provisioning;
    private readonly TenantSubscriptionService _sut;

    public TenantSubscriptionServiceTests()
    {
        _tenantRepo = new Mock<IPlatformRepository<Tenant>>();
        _subRepo = new Mock<IPlatformRepository<Subscription>>();
        _planRepo = new Mock<IPlatformRepository<Plan>>();
        _uow = new Mock<IPlatformUnitOfWork>();
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _provisioning = new Mock<ITenantProvisioningService>();

        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Database:PlatformConnectionString"]).Returns("Server=test;");

        _sut = new TenantSubscriptionService(
            _tenantRepo.Object, _subRepo.Object, _planRepo.Object,
            _uow.Object, _provisioning.Object, config.Object);
    }

    // ── ListTenants ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ListTenants_ReturnsTenantsOrderedByName()
    {
        var tenants = new List<Tenant>
        {
            new() { Id = "t2", Name = "Zebra FC" },
            new() { Id = "t1", Name = "Alpha SC" }
        };
        _tenantRepo.Setup(r => r.Query()).Returns(tenants.AsAsyncQueryable());

        var result = await _sut.ListTenantsAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.First().Name.Should().Be("Alpha SC");
    }

    // ── GetTenant ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTenant_ExistingId_ReturnsTenant()
    {
        var tenant = new Tenant { Id = "t1", Name = "Alpha SC" };
        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _sut.GetTenantAsync("t1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Alpha SC");
    }

    [Fact]
    public async Task GetTenant_NonExistingId_ReturnsNotFound()
    {
        _tenantRepo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _sut.GetTenantAsync("x", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    // ── CreateTenant ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("", "sub")]
    [InlineData("   ", "sub")]
    public async Task CreateTenant_EmptyName_ReturnsInvalid(string name, string sub)
    {
        var result = await _sut.CreateTenantAsync(name, sub, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    public async Task CreateTenant_EmptySubdomain_ReturnsInvalid(string name, string sub)
    {
        var result = await _sut.CreateTenantAsync(name, sub, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task CreateTenant_DuplicateSubdomain_ReturnsConflict()
    {
        var existing = new List<Tenant> { new() { Subdomain = "alpha" } };
        _tenantRepo.Setup(r => r.Query()).Returns(existing.AsAsyncQueryable());

        var result = await _sut.CreateTenantAsync("Alpha", "alpha", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task CreateTenant_ValidData_PersistsAndReturnsTenant()
    {
        _tenantRepo.Setup(r => r.Query()).Returns(new List<Tenant>().AsAsyncQueryable());
        _tenantRepo.Setup(r => r.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.CreateTenantAsync("My Club", "myclub", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My Club");
        result.Value.Subdomain.Should().Be("myclub");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── UpdateTenant ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTenant_NonExistingId_ReturnsNotFound()
    {
        _tenantRepo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _sut.UpdateTenantAsync("x", "N", "s", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task UpdateTenant_DuplicateSubdomainOnOtherTenant_ReturnsConflict()
    {
        var tenant = new Tenant { Id = "t1", Subdomain = "alpha" };
        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var others = new List<Tenant> { new() { Id = "t2", Subdomain = "beta" } };
        _tenantRepo.Setup(r => r.Query()).Returns(others.AsAsyncQueryable());

        var result = await _sut.UpdateTenantAsync("t1", "Alpha SC", "beta", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    [Fact]
    public async Task UpdateTenant_ValidData_UpdatesAndReturnsTenant()
    {
        var tenant = new Tenant { Id = "t1", Name = "Old", Subdomain = "old" };
        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepo.Setup(r => r.Query()).Returns(new List<Tenant>().AsAsyncQueryable());

        var result = await _sut.UpdateTenantAsync("t1", "New Name", "newslug", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Subdomain.Should().Be("newslug");
        _repo_Verify_Update(tenant);
    }

    private void _repo_Verify_Update(Tenant tenant)
        => _tenantRepo.Verify(r => r.Update(tenant), Times.Once);

    // ── DeleteTenant ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTenant_NonExistingId_ReturnsNotFound()
    {
        _tenantRepo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _sut.DeleteTenantAsync("x", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task DeleteTenant_ExistingId_RemovesAndReturnsSuccess()
    {
        var tenant = new Tenant { Id = "t1" };
        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _sut.DeleteTenantAsync("t1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tenantRepo.Verify(r => r.Remove(tenant), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── SubscribeTenant ──────────────────────────────────────────────────────

    [Fact]
    public async Task SubscribeTenant_TenantNotFound_ReturnsNotFound()
    {
        _tenantRepo.Setup(r => r.GetByIdAsync("t99", It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _sut.SubscribeTenantAsync("t99", "plan1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task SubscribeTenant_PlanNotFound_ReturnsNotFound()
    {
        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new Tenant { Id = "t1", DatabaseName = "BabaPlay_test" });
        _planRepo.Setup(r => r.GetByIdAsync("p99", It.IsAny<CancellationToken>())).ReturnsAsync((Plan?)null);

        var result = await _sut.SubscribeTenantAsync("t1", "p99", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task SubscribeTenant_ValidData_CreatesSubscriptionAndProvisions()
    {
        var tenant = new Tenant { Id = "t1", DatabaseName = "BabaPlay_abc" };
        var plan = new Plan { Id = "p1", Name = "Pro" };

        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _subRepo.Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _provisioning.Setup(p => p.ProvisionDatabaseAsync("BabaPlay_abc", "Server=test;", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success());

        var result = await _sut.SubscribeTenantAsync("t1", "p1", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be("t1");
        result.Value.PlanId.Should().Be("p1");
        _provisioning.Verify(p => p.ProvisionDatabaseAsync("BabaPlay_abc", "Server=test;", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubscribeTenant_ProvisioningFails_ReturnsFailure()
    {
        var tenant = new Tenant { Id = "t1", DatabaseName = "BabaPlay_abc" };
        var plan = new Plan { Id = "p1" };

        _tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepo.Setup(r => r.GetByIdAsync("p1", It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _subRepo.Setup(r => r.AddAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _provisioning.Setup(p => p.ProvisionDatabaseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure("DB error."));

        var result = await _sut.SubscribeTenantAsync("t1", "p1", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
