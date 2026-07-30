using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web;

public class UserSessionStateTests
{
    [Fact]
    public void SetUserSession_ShouldPopulatePropertiesAndSetIsAuthenticatedTrue()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        var token = "header.payload.signature";
        var userId = "user-123";
        var email = "atleta@babaplay.com";
        var fullName = "Carlos Silva";
        var roles = new List<string> { "Admin", "Player" };
        var eventFired = false;

        userSessionState.OnChange += () => eventFired = true;

        // Act
        userSessionState.SetUserSession(token, userId, email, fullName, roles);

        // Assert
        userSessionState.IsAuthenticated.Should().BeTrue();
        userSessionState.JwtToken.Should().Be(token);
        userSessionState.UserId.Should().Be(userId);
        userSessionState.Email.Should().Be(email);
        userSessionState.FullName.Should().Be(fullName);
        userSessionState.Roles.Should().Contain("Admin").And.Contain("Player");
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void Clear_ShouldResetSessionPropertiesAndSetIsAuthenticatedFalse()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        userSessionState.SetUserSession("token", "1", "email@test.com", "Name", new[] { "Admin" });
        var eventFired = false;

        userSessionState.OnChange += () => eventFired = true;

        // Act
        userSessionState.Clear();

        // Assert
        userSessionState.IsAuthenticated.Should().BeFalse();
        userSessionState.JwtToken.Should().BeNull();
        userSessionState.UserId.Should().BeNull();
        userSessionState.Email.Should().BeNull();
        userSessionState.Roles.Should().BeEmpty();
        eventFired.Should().BeTrue();
    }
}
