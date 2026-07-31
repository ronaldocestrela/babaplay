using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using BabaPlay.Web.Services.Handlers;
using BabaPlay.Web.Services.State;

namespace BabaPlay.Tests.Web;

public class AuthorizationHeaderHandlerTests
{
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task SendAsync_WhenSessionAndTenantActive_ShouldAppendHeaders()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();

        var token = "jwt-test-token";
        var tenantId = Guid.NewGuid();
        const string tenantSlug = "test-tenant";

        userSessionState.SetUserSession(token, "user-1", "test@test.com", "Test User");
        tenantState.SetTenant(tenantId, "Test Tenant", tenantSlug);

        var innerHandler = new TestHttpMessageHandler();
        var handler = new AuthorizationHeaderHandler(userSessionState, tenantState)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler);

        // Act
        await client.GetAsync("http://localhost/api/v1/test");

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        innerHandler.LastRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        innerHandler.LastRequest.Headers.Authorization.Parameter.Should().Be(token);

        innerHandler.LastRequest.Headers.Contains("X-Tenant-Slug").Should().BeTrue();
        innerHandler.LastRequest.Headers.GetValues("X-Tenant-Slug").FirstOrDefault().Should().Be(tenantSlug);
        innerHandler.LastRequest.Headers.Contains("X-Tenant-Id").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenNoSession_ShouldNotAppendAuthHeaders()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();

        var innerHandler = new TestHttpMessageHandler();
        var handler = new AuthorizationHeaderHandler(userSessionState, tenantState)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler);

        // Act
        await client.GetAsync("http://localhost/api/v1/test");

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().BeNull();
        innerHandler.LastRequest.Headers.Contains("X-Tenant-Slug").Should().BeFalse();
        innerHandler.LastRequest.Headers.Contains("X-Tenant-Id").Should().BeFalse();
    }

    [Fact]
    public async Task SendAsync_WhenTenantHasIdButNoSlug_ShouldNotAppendTenantHeader()
    {
        // Arrange
        var userSessionState = new UserSessionState();
        var tenantState = new TenantState();

        userSessionState.SetUserSession("jwt-test-token", "user-1", "test@test.com", "Test User");
        tenantState.SetTenant(Guid.NewGuid(), "Test Tenant");

        var innerHandler = new TestHttpMessageHandler();
        var handler = new AuthorizationHeaderHandler(userSessionState, tenantState)
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(handler);

        // Act
        await client.GetAsync("http://localhost/api/v1/test");

        // Assert
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Contains("X-Tenant-Slug").Should().BeFalse();
        innerHandler.LastRequest.Headers.Contains("X-Tenant-Id").Should().BeFalse();
    }
}
