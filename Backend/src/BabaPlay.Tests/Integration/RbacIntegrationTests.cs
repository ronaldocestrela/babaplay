using System.Net;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// End-to-end tests for RBAC permission policies and tenant isolation.
/// </summary>
public sealed class RbacIntegrationTests : IClassFixture<RbacWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RbacIntegrationTests(RbacWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", "test-token");
    }

    [Fact]
    public async Task GetRoles_AdminWithPermissionInTenantA_ShouldReturn200()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/role");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoles_MemberWithoutPermissionInTenantA_ShouldReturn403()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/role");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.MemberUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRoles_AdminUserOutsideTenantB_ShouldReturn403()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/role");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantBSlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMonthlySummary_AdminWithPermissionInTenantA_ShouldReturn200()
    {
        var now = DateTime.UtcNow;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/financial/monthly-summary?year={now.Year}&month={now.Month}");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMonthlySummary_MemberWithoutPermissionInTenantA_ShouldReturn403()
    {
        var now = DateTime.UtcNow;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/financial/monthly-summary?year={now.Year}&month={now.Month}");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.MemberUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
