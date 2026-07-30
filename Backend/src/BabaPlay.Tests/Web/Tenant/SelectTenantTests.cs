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
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Web.Tenant;

public class SelectTenantTests : TestContext
{
    [Fact]
    public void SelectTenant_WhenMembershipsExist_ShouldRenderTenantCards()
    {
        // Arrange
        var mockTenantService = new Mock<ITenantApiService>();
        var tenantState = new TenantState();

        var memberships = new List<TenantSummaryDto>
        {
            new(Guid.NewGuid(), "Baba do Resenha", "baba-resenha", null, "Admin"),
            new(Guid.NewGuid(), "Liga dos Amigos", "liga-amigos", null, "Player")
        };

        mockTenantService
            .Setup(x => x.GetMyMembershipsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberships);

        Services.AddSingleton(mockTenantService.Object);
        Services.AddSingleton(tenantState);

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
        var tenantState = new TenantState();

        mockTenantService
            .Setup(x => x.GetMyMembershipsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantSummaryDto>());

        Services.AddSingleton(mockTenantService.Object);
        Services.AddSingleton(tenantState);

        // Act
        var cut = RenderComponent<SelectTenant>();

        // Assert
        cut.Markup.Should().Contain("Você ainda não faz parte de nenhuma associação.");
        cut.Find("#create-first-tenant-btn").Should().NotBeNull();
    }
}
