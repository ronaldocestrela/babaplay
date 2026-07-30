using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web;

public class CustomAuthStateProviderTests
{
    [Fact]
    public async Task GetAuthenticationStateAsync_WhenNoUserSession_ShouldReturnUnauthenticated()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        var provider = new CustomAuthStateProvider(userSessionState);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity.Should().NotBeNull();
        state.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenUserSessionActive_ShouldReturnAuthenticatedWithClaims()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        var provider = new CustomAuthStateProvider(userSessionState);

        userSessionState.SetUserSession(
            jwtToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c",
            userId: "user-99",
            email: "atleta@baba.com",
            fullName: "John Doe",
            roles: new[] { "Admin" }
        );

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity.Should().NotBeNull();
        state.User.Identity!.IsAuthenticated.Should().BeTrue();
        state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-99");
        state.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be("atleta@baba.com");
        state.User.IsInRole("Admin").Should().BeTrue();
    }
}
