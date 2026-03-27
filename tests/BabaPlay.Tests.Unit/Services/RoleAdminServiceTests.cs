using BabaPlay.Modules.Identity;
using BabaPlay.Modules.Identity.Entities;
using BabaPlay.Modules.Identity.Services;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.Tests.Unit.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace BabaPlay.Tests.Unit.Services;

public sealed class RoleAdminServiceTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManager;
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<ITenantRepository<Permission>> _permRepo;
    private readonly RoleAdminService _sut;

    public RoleAdminServiceTests()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManager = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null!, null!, null!, null!);

        var userStore = new Mock<IUserStore<ApplicationUser>>();
        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object, null!, null!,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            null!, null!, null!,
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        _permRepo = new Mock<ITenantRepository<Permission>>();

        _sut = new RoleAdminService(_roleManager.Object, _userManager.Object, _permRepo.Object);
    }

    // ── ListRoles ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListRoles_ReturnsOrderedRoleNames()
    {
        var roles = new List<ApplicationRole>
        {
            new() { Name = "Manager" },
            new() { Name = "Admin" },
            new() { Name = "Associate" }
        };
        _roleManager.Setup(r => r.Roles).Returns(roles.AsAsyncQueryable());

        var result = await _sut.ListRolesAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeInAscendingOrder();
        result.Value.Should().Contain("Admin").And.Contain("Manager");
    }

    // ── AssignRole ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignRole_UserNotFound_ReturnsNotFound()
    {
        _userManager.Setup(u => u.FindByIdAsync("u999")).ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.AssignRoleAsync("u999", "Admin", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task AssignRole_RoleNotFound_ReturnsNotFound()
    {
        var user = new ApplicationUser { Id = "u1" };
        _userManager.Setup(u => u.FindByIdAsync("u1")).ReturnsAsync(user);
        _roleManager.Setup(r => r.RoleExistsAsync("Ghost")).ReturnsAsync(false);

        var result = await _sut.AssignRoleAsync("u1", "Ghost", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task AssignRole_AlreadyHasRole_ReturnsSuccess()
    {
        var user = new ApplicationUser { Id = "u1" };
        _userManager.Setup(u => u.FindByIdAsync("u1")).ReturnsAsync(user);
        _roleManager.Setup(r => r.RoleExistsAsync("Associate")).ReturnsAsync(true);
        _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(["Associate"]);

        var result = await _sut.AssignRoleAsync("u1", "Associate", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userManager.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AssignRole_ValidData_AddsRoleAndReturnsSuccess()
    {
        var user = new ApplicationUser { Id = "u1" };
        _userManager.Setup(u => u.FindByIdAsync("u1")).ReturnsAsync(user);
        _roleManager.Setup(r => r.RoleExistsAsync("Manager")).ReturnsAsync(true);
        _userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync([]);
        _userManager.Setup(u => u.AddToRoleAsync(user, "Manager")).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.AssignRoleAsync("u1", "Manager", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userManager.Verify(u => u.AddToRoleAsync(user, "Manager"), Times.Once);
    }

    // ── ListPermissions ──────────────────────────────────────────────────────

    [Fact]
    public async Task ListPermissions_ReturnsOrderedPermissions()
    {
        var perms = new List<Permission>
        {
            new() { Name = "reports.view" },
            new() { Name = "associates.manage" }
        };
        _permRepo.Setup(r => r.Query()).Returns(perms.AsAsyncQueryable());

        var result = await _sut.ListPermissionsAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.First().Name.Should().Be("associates.manage");
    }
}
