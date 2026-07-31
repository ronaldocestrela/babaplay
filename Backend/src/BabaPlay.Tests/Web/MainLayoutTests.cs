using Bunit;
using Bunit.TestDoubles;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using BabaPlay.Web.Components.Layout;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web;

public class MainLayoutTests : TestContext
{
    [Fact]
    public void MainLayout_ShouldRenderSidebarHeaderAndContent()
    {
        // Arrange
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("Test User");

        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();
        var authStateProvider = new CustomAuthStateProvider(userSessionState);

        var mockNotificationService = new Moq.Mock<BabaPlay.Web.Services.Http.INotificationApiService>();
        Services.AddSingleton(tenantState);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(authStateProvider);
        Services.AddSingleton(mockNotificationService.Object);

        // Act
        var cut = RenderComponent<MainLayout>();

        // Assert
        cut.Markup.Should().Contain("Baba Play");
        cut.Markup.Should().Contain("Associação:");
    }
}
