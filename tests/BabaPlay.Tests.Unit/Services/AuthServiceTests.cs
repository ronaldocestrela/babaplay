using BabaPlay.Modules.Identity;
using BabaPlay.Modules.Identity.Services;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace BabaPlay.Tests.Unit.Services;

public sealed class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _users;
    private readonly Mock<IPermissionResolver> _permissions;
    private readonly Mock<IAccessTokenIssuer> _tokens;
    private readonly Mock<IAssociateStatusChecker> _associateStatus;
    private readonly Mock<IAssociateSignupSynchronizer> _associateSignup;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        store.As<IUserPasswordStore<ApplicationUser>>();
        store.As<IUserEmailStore<ApplicationUser>>();

        _users = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        _permissions = new Mock<IPermissionResolver>();
        _tokens = new Mock<IAccessTokenIssuer>();
        _associateStatus = new Mock<IAssociateStatusChecker>();
        _associateSignup = new Mock<IAssociateSignupSynchronizer>();
        _associateStatus
            .Setup(s => s.IsActiveByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _associateSignup
            .Setup(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success("assoc-1"));
        _associateSignup
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _sut = new AuthService(_users.Object, _permissions.Object, _tokens.Object, _associateStatus.Object, _associateSignup.Object);
    }

    // ── Register ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_EmptyName_ReturnsInvalid()
    {
        var result = await _sut.RegisterAsync("  ", "user@test.com", "P@ssw0rd!", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Register_EmptyEmail_ReturnsInvalid()
    {
        var result = await _sut.RegisterAsync("User", "  ", "P@ssw0rd!", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Register_EmptyPassword_ReturnsInvalid()
    {
        var result = await _sut.RegisterAsync("User", "user@test.com", "  ", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Register_IdentityFailure_ReturnsInvalid()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        var result = await _sut.RegisterAsync("User", "user@test.com", "weak", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Errors.Should().Contain("Password too weak.");
    }

    [Fact]
    public async Task Register_Associate_ValidData_CreatesAssociateAndReturnsAuthResponse()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(["Associate"]);
        _permissions.Setup(p => p.GetPermissionNamesForUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);
        _tokens.Setup(t => t.Issue(It.IsAny<IReadOnlyCollection<Claim>>()))
               .Returns("access-token");

        var result = await _sut.RegisterAsync("User", "user@test.com", "P@ssw0rd!", UserType.Associate);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        _associateSignup.Verify(
            s => s.CreateAsync("User", "user@test.com", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _users.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Register_AssociationStaff_ValidData_AssignsManagerAndCreatesAssociate()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(["Manager"]);
        _permissions.Setup(p => p.GetPermissionNamesForUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);
        _tokens.Setup(t => t.Issue(It.IsAny<IReadOnlyCollection<Claim>>()))
               .Returns("access-token");

        var result = await _sut.RegisterAsync("Manager Name", "manager@test.com", "P@ssw0rd!", UserType.AssociationStaff);

        result.IsSuccess.Should().BeTrue();
        _users.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Manager"), Times.Once);
        _associateSignup.Verify(
            s => s.CreateAsync("Manager Name", "manager@test.com", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_PlatformAdmin_DoesNotCreateAssociate()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(["Admin"]);
        _permissions.Setup(p => p.GetPermissionNamesForUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);
        _tokens.Setup(t => t.Issue(It.IsAny<IReadOnlyCollection<Claim>>()))
               .Returns("access-token");

        var result = await _sut.RegisterAsync("Platform", "platform@test.com", "P@ssw0rd!", UserType.PlatformAdmin);

        result.IsSuccess.Should().BeTrue();
        _associateSignup.Verify(
            s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _users.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenRoleAssignmentFails_DeletesUserAndSkipsAssociateCreation()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role assignment failed." }));
        _users.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync("User", "user@test.com", "P@ssw0rd!", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.Errors.Should().Contain("Role assignment failed.");
        _associateSignup.Verify(
            s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _users.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Register_Associate_WhenAssociateCreationFails_DeletesUserAndReturnsInvalid()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _associateSignup.Setup(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result.Invalid<string>("Could not create associate."));
        _users.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync("User", "user@test.com", "P@ssw0rd!", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        _users.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task Register_Associate_WhenUserUpdateFails_RollsBackAssociateAndUser()
    {
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
              .ReturnsAsync(IdentityResult.Success);
        _users.Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed." }));
        _users.Setup(u => u.DeleteAsync(It.IsAny<ApplicationUser>()))
              .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync("User", "user@test.com", "P@ssw0rd!", UserType.Associate);

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
        _associateSignup.Verify(s => s.DeleteAsync("assoc-1", It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_EmptyEmail_ReturnsInvalid()
    {
        var result = await _sut.LoginAsync("", "P@ssw0rd!");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        _users.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
              .ReturnsAsync((ApplicationUser?)null);

        var result = await _sut.LoginAsync("ghost@test.com", "P@ssw0rd!");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var user = new ApplicationUser { Id = "u1", Email = "user@test.com" };
        _users.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "wrong")).ReturnsAsync(false);

        var result = await _sut.LoginAsync("user@test.com", "wrong");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Unauthorized);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        var user = new ApplicationUser { Id = "u1", Email = "user@test.com", UserName = "user@test.com" };
        _users.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "P@ssw0rd!")).ReturnsAsync(true);
        _users.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(["Associate"]);
        _permissions.Setup(p => p.GetPermissionNamesForUserAsync("u1", It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);
        _tokens.Setup(t => t.Issue(It.IsAny<IReadOnlyCollection<Claim>>())).Returns("jwt-token");

        var result = await _sut.LoginAsync("user@test.com", "P@ssw0rd!");

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("jwt-token");
        result.Value.UserId.Should().Be("u1");
        result.Value.Roles.Should().Contain("Associate");
    }

    [Fact]
    public async Task Login_InactiveAssociate_ReturnsForbidden()
    {
        var user = new ApplicationUser { Id = "u1", Email = "user@test.com", UserName = "user@test.com" };
        _users.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "P@ssw0rd!")).ReturnsAsync(true);
        _associateStatus.Setup(s => s.IsActiveByUserIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.LoginAsync("user@test.com", "P@ssw0rd!");

        result.IsFailure.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Forbidden);
        result.Error.Should().Contain("inactive");
        _tokens.Verify(t => t.Issue(It.IsAny<IReadOnlyCollection<Claim>>()), Times.Never);
    }

    [Fact]
    public async Task Login_ActiveAssociate_CallsCheckerAndReturnsToken()
    {
        var user = new ApplicationUser { Id = "u1", Email = "user@test.com", UserName = "user@test.com" };
        _users.Setup(u => u.FindByEmailAsync("user@test.com")).ReturnsAsync(user);
        _users.Setup(u => u.CheckPasswordAsync(user, "P@ssw0rd!")).ReturnsAsync(true);
        _associateStatus.Setup(s => s.IsActiveByUserIdAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _users.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(["Associate"]);
        _permissions.Setup(p => p.GetPermissionNamesForUserAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _tokens.Setup(t => t.Issue(It.IsAny<IReadOnlyCollection<Claim>>())).Returns("jwt-token");

        var result = await _sut.LoginAsync("user@test.com", "P@ssw0rd!");

        result.IsSuccess.Should().BeTrue();
        _associateStatus.Verify(s => s.IsActiveByUserIdAsync("u1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
