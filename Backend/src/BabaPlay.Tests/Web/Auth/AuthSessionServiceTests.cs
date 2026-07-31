using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Services.Http;
using BabaPlay.Web.Services.State;
using BabaPlay.Web.Services.Storage;

namespace BabaPlay.Tests.Web.Auth;

public class AuthSessionServiceTests
{
    [Fact]
    public async Task SaveCurrentSessionAsync_ShouldPersistSnapshotFromCurrentState()
    {
        // Arrange
        AuthSessionSnapshot? savedSnapshot = null;
        var storage = new Mock<IAuthSessionStorage>();
        storage
            .Setup(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>()))
            .Callback<AuthSessionSnapshot, CancellationToken>((snapshot, _) => savedSnapshot = snapshot)
            .Returns(Task.CompletedTask);

        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var authStateProvider = new CustomAuthStateProvider(userSessionState, tenantState);
        var authApiService = new Mock<IAuthApiService>();

        userSessionState.SetUserSession("access-token", "user-1", "user@baba.com", "User Baba", ["Admin"], "refresh-token");
        tenantState.SetTenant(Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), "Baba FC", "baba-fc", isOwner: true);

        var service = new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            authStateProvider,
            authApiService.Object);

        // Act
        await service.SaveCurrentSessionAsync();

        // Assert
        savedSnapshot.Should().NotBeNull();
        savedSnapshot!.AccessToken.Should().Be("access-token");
        savedSnapshot.RefreshToken.Should().Be("refresh-token");
        savedSnapshot.Email.Should().Be("user@baba.com");
        savedSnapshot.TenantSlug.Should().Be("baba-fc");
        savedSnapshot.TenantIsOwner.Should().BeTrue();
    }

    [Fact]
    public async Task RestoreSessionAsync_ShouldRehydrateUserAndTenantState()
    {
        // Arrange
        var tenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var snapshot = new AuthSessionSnapshot
        {
            AccessToken = CreateJwt("user-1", "user@baba.com", ["Admin"], expiresInSeconds: 3600),
            RefreshToken = "refresh-token",
            UserId = "user-1",
            Email = "user@baba.com",
            FullName = "User Baba",
            Roles = ["Admin"],
            TenantId = tenantId,
            TenantName = "Baba FC",
            TenantSlug = "baba-fc",
            TenantIsOwner = true,
        };

        var storage = new Mock<IAuthSessionStorage>();
        storage.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(snapshot);

        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var authStateProvider = new CustomAuthStateProvider(userSessionState, tenantState);
        var authApiService = new Mock<IAuthApiService>();
        authApiService
            .Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto(
                "user-1",
                "user@baba.com",
                ["Admin"],
                true,
                DateTime.UtcNow,
                new AuthTenantMembershipDto(tenantId, "Baba FC", "baba-fc", true, DateTime.UtcNow),
                [new AuthTenantMembershipDto(tenantId, "Baba FC", "baba-fc", true, DateTime.UtcNow)]));

        var service = new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            authStateProvider,
            authApiService.Object);

        // Act
        await service.RestoreSessionAsync();

        // Assert
        userSessionState.IsAuthenticated.Should().BeTrue();
        userSessionState.Email.Should().Be("user@baba.com");
        tenantState.CurrentTenantId.Should().Be(tenantId);
        tenantState.CurrentTenantSlug.Should().Be("baba-fc");
        tenantState.IsOwner.Should().BeTrue();
    }

    [Fact]
    public async Task RestoreSessionAsync_WhenSnapshotMissingOwnerFlag_ShouldSyncIsOwnerFromProfile()
    {
        var tenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var snapshot = new AuthSessionSnapshot
        {
            AccessToken = CreateJwt("user-1", "user@baba.com", ["Admin"], expiresInSeconds: 3600),
            RefreshToken = "refresh-token",
            UserId = "user-1",
            Email = "user@baba.com",
            FullName = "User Baba",
            Roles = ["Admin"],
            TenantId = tenantId,
            TenantName = "Baba FC",
            TenantSlug = "baba-fc",
            TenantIsOwner = false,
        };

        var storage = new Mock<IAuthSessionStorage>();
        storage.Setup(x => x.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(snapshot);
        storage.Setup(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var authStateProvider = new CustomAuthStateProvider(userSessionState, tenantState);
        var authApiService = new Mock<IAuthApiService>();
        authApiService
            .Setup(x => x.GetMeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto(
                "user-1",
                "user@baba.com",
                ["Admin"],
                true,
                DateTime.UtcNow,
                new AuthTenantMembershipDto(tenantId, "Baba FC", "baba-fc", true, DateTime.UtcNow),
                [new AuthTenantMembershipDto(tenantId, "Baba FC", "baba-fc", true, DateTime.UtcNow)]));

        var service = new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            authStateProvider,
            authApiService.Object);

        await service.RestoreSessionAsync();

        tenantState.IsOwner.Should().BeTrue();
        storage.Verify(x => x.SaveAsync(It.Is<AuthSessionSnapshot>(s => s.TenantIsOwner), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ClearSessionAsync_ShouldClearMemoryAndStorage()
    {
        // Arrange
        var storage = new Mock<IAuthSessionStorage>();
        storage.Setup(x => x.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var authStateProvider = new CustomAuthStateProvider(userSessionState, tenantState);
        var authApiService = new Mock<IAuthApiService>();

        userSessionState.SetUserSession("access-token", "user-1", "user@baba.com", "User Baba");
        tenantState.SetTenant(Guid.NewGuid(), "Baba FC", "baba-fc");

        var service = new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            authStateProvider,
            authApiService.Object);

        // Act
        await service.ClearSessionAsync();

        // Assert
        userSessionState.IsAuthenticated.Should().BeFalse();
        tenantState.CurrentTenantId.Should().BeNull();
        storage.Verify(x => x.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static string CreateJwt(string userId, string email, IReadOnlyList<string> roles, int expiresInSeconds)
    {
        var exp = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds).ToUnixTimeSeconds();
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var payloadJson = $$"""
            {"sub":"{{userId}}","email":"{{email}}","role":["{{string.Join("\",\"", roles)}}"],"exp":{{exp}}}
            """;
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{header}.{payload}.signature";
    }
}
