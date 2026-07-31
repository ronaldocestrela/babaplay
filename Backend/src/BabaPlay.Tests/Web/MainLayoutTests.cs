using Bunit;
using Bunit.TestDoubles;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using BabaPlay.Web.Components.Layout;
using BabaPlay.Web.Services.Http;
using BabaPlay.Web.Services.State;
using BabaPlay.Web.Services.Storage;

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
        var authStateProvider = new CustomAuthStateProvider(userSessionState, tenantState);
        var storage = new Mock<IAuthSessionStorage>();
        var authApi = new Mock<IAuthApiService>();
        var authSessionService = new AuthSessionService(
            storage.Object,
            userSessionState,
            tenantState,
            authStateProvider,
            authApi.Object);

        var mockNotificationService = new Mock<INotificationApiService>();
        Services.AddSingleton(tenantState);
        Services.AddSingleton(userSessionState);
        Services.AddSingleton(authStateProvider);
        Services.AddSingleton(authSessionService);
        Services.AddSingleton(mockNotificationService.Object);

        // Act
        var cut = RenderComponent<MainLayout>();

        // Assert
        cut.Markup.Should().Contain("Baba Play");
        cut.Markup.Should().Contain("Associação:");
    }
}
