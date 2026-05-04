using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class RankingIntegrationTests : IClassFixture<RbacWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RankingIntegrationTests(RbacWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new("Bearer", "test-token");
    }

    [Fact]
    public async Task GetRanking_AdminWithPermissionInTenantA_ShouldReturn200()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ranking");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRanking_MemberWithoutPermissionInTenantA_ShouldReturn403()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/ranking");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.MemberUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostRebuild_AdminWithPermissionInTenantA_ShouldReturn200()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ranking/rebuild")
        {
            Content = JsonContent.Create(new { fromUtc = (DateTime?)null, toUtc = (DateTime?)null })
        };

        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostRebuild_MemberWithoutPermissionInTenantA_ShouldReturn403()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ranking/rebuild")
        {
            Content = JsonContent.Create(new { fromUtc = (DateTime?)null, toUtc = (DateTime?)null })
        };

        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", RbacWebApplicationFactory.TenantASlug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.MemberUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostRebuild_InvalidTenantSlug_ShouldReturn404WithTenantNotFound()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ranking/rebuild")
        {
            Content = JsonContent.Create(new { fromUtc = (DateTime?)null, toUtc = (DateTime?)null })
        };

        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", "tenant-does-not-exist");
        request.Headers.Add(TestAuthHandler.UserIdHeader, RbacWebApplicationFactory.AdminUserId);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("title").GetString().Should().Be("TENANT_NOT_FOUND");
    }
}