using System.Net;
using System.Net.Http.Json;
using BabaPlay.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// End-to-end integration tests for Phase 2 tenant endpoints.
/// Covers: create tenant, duplicate slug, status query, TenantMiddleware header resolution.
/// Auth is handled by <see cref="TestAuthHandler"/> (always authenticated).
/// </summary>
public class TenantIntegrationTests : IClassFixture<TenantWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TenantIntegrationTests(TenantWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── POST /api/v1/tenant ────────────────────────────────────────────────

    [Fact]
    public async Task POST_Tenant_ValidRequest_ShouldReturn201WithPendingStatus()
    {
        // Arrange
        var slug = $"club-{Guid.NewGuid():N}"[..20];

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/tenant", new
        {
            Name = "Test Club",
            Slug = slug,
            AdminEmail = "owner@testclub.com",
            AdminPassword = "TestOwner@123456",
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Test Club");
        body.Slug.Should().Be(slug);
        body.ProvisioningStatus.Should().Be("Pending");
        body.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task POST_Tenant_DuplicateSlug_ShouldReturn409()
    {
        // Arrange
        var slug = $"dup-{Guid.NewGuid():N}"[..20];
        await _client.PostAsJsonAsync("/api/v1/tenant", new
        {
            Name = "Club A",
            Slug = slug,
            AdminEmail = "owner-a@testclub.com",
            AdminPassword = "TestOwner@123456",
        });

        // Act — second request with same slug
        var response = await _client.PostAsJsonAsync("/api/v1/tenant", new
        {
            Name = "Club B",
            Slug = slug,
            AdminEmail = "owner-b@testclub.com",
            AdminPassword = "TestOwner@123456",
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("TENANT_SLUG_TAKEN");
    }

    [Fact]
    public async Task POST_Tenant_EmptyName_ShouldReturn422()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/tenant", new { Name = "", Slug = "some-slug" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("TENANT_NAME_REQUIRED");
    }

    // ── GET /api/v1/tenant/{id}/status ─────────────────────────────────────

    [Fact]
    public async Task GET_TenantStatus_KnownId_ShouldReturn200WithStatus()
    {
        // Arrange — create a tenant first
        var slug = $"status-{Guid.NewGuid():N}"[..20];
        var createResp = await _client.PostAsJsonAsync("/api/v1/tenant", new
        {
            Name = "Status Club",
            Slug = slug,
            AdminEmail = "owner-status@testclub.com",
            AdminPassword = "TestOwner@123456",
        });
        var created = await createResp.Content.ReadFromJsonAsync<TenantResponse>();

        // Act
        var response = await _client.GetAsync($"/api/v1/tenant/{created!.Id}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body!.Id.Should().Be(created.Id);
        body.ProvisioningStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task GET_TenantStatus_UnknownId_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/tenant/{Guid.NewGuid()}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── TenantMiddleware ────────────────────────────────────────────────────

    [Fact]
    public async Task Request_WithValidTenantSlugHeader_ShouldResolveAndSucceed()
    {
        // Arrange — create a tenant so its slug exists in the DB
        var slug = $"mw-{Guid.NewGuid():N}"[..20];
        var createResp = await _client.PostAsJsonAsync("/api/v1/tenant", new
        {
            Name = "MW Club",
            Slug = slug,
            AdminEmail = "owner-mw@testclub.com",
            AdminPassword = "TestOwner@123456",
        });
        var created = await createResp.Content.ReadFromJsonAsync<TenantResponse>();

        // Act — include X-Tenant-Slug on the status request
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tenant/{created!.Id}/status");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", slug);

        var response = await _client.SendAsync(request);

        // Assert — middleware resolves slug; controller returns 200 normally
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Request_WithInvalidTenantSlugHeader_ShouldReturn404()
    {
        // Act — send a request with a slug that doesn't exist in the DB
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/tenant/{Guid.NewGuid()}/status");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", "totally-nonexistent-slug");

        var response = await _client.SendAsync(request);

        // Assert — TenantMiddleware throws NotFoundException → 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

