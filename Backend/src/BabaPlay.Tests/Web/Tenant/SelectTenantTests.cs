using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using Moq;
using Xunit;
using BabaPlay.Web.Models;
using BabaPlay.Web.Pages.Tenant;
using BabaPlay.Web.Services.Http;
using BabaPlay.Web.Services.State;
using BabaPlay.Web.Services.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Tenant;

public class SelectTenantTests : TestContext
{
    private void RegisterServices(Mock<ITenantApiService> mockTenantService)
    {
        var tenantState = new TenantState();
        var userSessionState = new UserSessionState();
        var customAuthStateProvider = new CustomAuthStateProvider(userSessionState, tenantState);
        var mockStorage = new Mock<IAuthSessionStorage>();
        var mockAuthApi = new Mock<IAuthApiService>();

        mockStorage.Setup(x => x.SaveAsync(It.IsAny<AuthSessionSnapshot>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockStorage.Setup(x => x.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var authSessionService = new AuthSessionService(
            mockStorage.Object,
            userSessionState,
            tenantState,
            customAuthStateProvider,
            mockAuthApi.Object);

        Services.AddSingleton(mockTenantService.Object);
        Services.AddSingleton(tenantState);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(customAuthStateProvider);
        Services.AddSingleton<AuthenticationStateProvider>(customAuthStateProvider);
        Services.AddSingleton(authSessionService);
    }

    [Fact]
    public void SelectTenant_WhenMembershipsExist_ShouldRenderTenantCards()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();

        var memberships = new List<TenantSummaryDto>
        {
            new(Guid.NewGuid(), "Baba do Resenha", "baba-resenha", null, "Admin"),
            new(Guid.NewGuid(), "Liga dos Amigos", "liga-amigos", null, "Player")
        };

        mockTenantService
            .Setup(x => x.GetMyMembershipsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberships);

        RegisterServices(mockTenantService);

        // Act
        var cut = RenderComponent<SelectTenant>();

        // Assert
        cut.FindAll(".group").Count.Should().Be(2);
        cut.Markup.Should().Contain("Baba do Resenha");
        cut.Markup.Should().Contain("Liga dos Amigos");
    }

    [Fact]
    public void SelectTenant_WhenNoMemberships_ShouldRenderEmptyState()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();

        mockTenantService
            .Setup(x => x.GetMyMembershipsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantSummaryDto>());

        RegisterServices(mockTenantService);

        // Act
        var cut = RenderComponent<SelectTenant>();

        // Assert
        cut.Markup.Should().Contain("Você ainda não faz parte de nenhuma associação.");
        cut.Find("#create-first-tenant-btn").Should().NotBeNull();
    }
}

