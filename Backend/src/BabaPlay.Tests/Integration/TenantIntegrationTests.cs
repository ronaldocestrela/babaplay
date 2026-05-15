using System.Net;
using System.Net.Http.Json;
using BabaPlay.Application.DTOs;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// End-to-end integration tests for Phase 2 tenant endpoints.
/// Covers: create tenant, duplicate slug, status query, TenantMiddleware header resolution.
/// Auth is handled by <see cref="TestAuthHandler"/> (always authenticated).
/// </summary>
public class TenantIntegrationTests : IClassFixture<TenantWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TenantWebApplicationFactory _factory;

    private static MultipartFormDataContent BuildTenantCreateContent(string name, string slug)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent(slug), "Slug" },
            { new StringContent("owner@testclub.com"), "AdminEmail" },
            { new StringContent("TestOwner@123456"), "AdminPassword" },
            { new StringContent("Rua das Palmeiras"), "Street" },
            { new StringContent("123"), "Number" },
            { new StringContent("Centro"), "Neighborhood" },
            { new StringContent("Sao Paulo"), "City" },
            { new StringContent("SP"), "State" },
            { new StringContent("01000-000"), "ZipCode" },
            { new StringContent("-23.5505"), "AssociationLatitude" },
            { new StringContent("-46.6333"), "AssociationLongitude" },
        };

        var logoBytes = new byte[] { 1, 2, 3, 4 };
        var logoContent = new ByteArrayContent(logoBytes);
        logoContent.Headers.ContentType = new("image/png");
        content.Add(logoContent, "Logo", "logo.png");

        return content;
    }

    public TenantIntegrationTests(TenantWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static MultipartFormDataContent BuildTenantSettingsUpdateContent(string name)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent("11"), "PlayersPerTeam" },
            { new StringContent("Rua Atualizada"), "Street" },
            { new StringContent("321"), "Number" },
            { new StringContent("Bairro Novo"), "Neighborhood" },
            { new StringContent("Rio de Janeiro"), "City" },
            { new StringContent("RJ"), "State" },
            { new StringContent("20000-000"), "ZipCode" },
            { new StringContent("-22.9068"), "AssociationLatitude" },
            { new StringContent("-43.1729"), "AssociationLongitude" },
        };

        var logoBytes = new byte[] { 9, 8, 7, 6 };
        var logoContent = new ByteArrayContent(logoBytes);
        logoContent.Headers.ContentType = new("image/png");
        content.Add(logoContent, "Logo", "new-logo.png");

        return content;
    }

    private sealed record TenantGameDayOptionResponse(
        Guid Id,
        Guid TenantId,
        DayOfWeek DayOfWeek,
        TimeOnly LocalStartTime,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    // ── POST /api/v1/tenant ────────────────────────────────────────────────

    [Fact]
    public async Task POST_Tenant_ValidRequest_ShouldReturn201WithPendingStatus()
    {
        // Arrange
        var slug = $"club-{Guid.NewGuid():N}"[..20];

        // Act
        var response = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Test Club", slug));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Test Club");
        body.Slug.Should().Be(slug);
        body.ProvisioningStatus.Should().Be("Pending");
        body.PlayersPerTeam.Should().Be(11);
        body.Id.Should().NotBeEmpty();
        body.LogoPath.Should().NotBeNullOrWhiteSpace();
        body.LogoPath.Should().StartWith("https://res.cloudinary.com/");
        body.Street.Should().Be("Rua das Palmeiras");
        body.Number.Should().Be("123");
        body.City.Should().Be("Sao Paulo");
        body.State.Should().Be("SP");
        body.ZipCode.Should().Be("01000-000");
        body.AssociationLatitude.Should().Be(-23.5505);
        body.AssociationLongitude.Should().Be(-46.6333);
    }

    [Fact]
    public async Task POST_Tenant_DuplicateSlug_ShouldReturn409()
    {
        // Arrange
        var slug = $"dup-{Guid.NewGuid():N}"[..20];
        await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Club A", slug));

        // Act — second request with same slug
        var response = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Club B", slug));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("TENANT_SLUG_TAKEN");
    }

    [Fact]
    public async Task POST_Tenant_EmptyName_ShouldReturn422()
    {
        // Act
        var response = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("", "some-slug"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("TENANT_NAME_REQUIRED");
    }

    [Fact]
    public async Task POST_Tenant_WithInvalidTenantSlugHeader_ShouldStillReturn201()
    {
        // Arrange
        var slug = $"club-header-{Guid.NewGuid():N}"[..20];
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant")
        {
            Content = BuildTenantCreateContent("Header Club", slug),
        };
        request.Headers.Add("X-Tenant-Slug", "totally-nonexistent-slug");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // ── GET /api/v1/tenant/{id}/status ─────────────────────────────────────

    [Fact]
    public async Task GET_TenantStatus_KnownId_ShouldReturn200WithStatus()
    {
        // Arrange — create a tenant first
        var slug = $"status-{Guid.NewGuid():N}"[..20];
        var createResp = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Status Club", slug));
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
        var createResp = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("MW Club", slug));
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

    [Fact]
    public async Task GET_TenantSettings_AsMember_ShouldReturn200()
    {
        var slug = $"settings-{Guid.NewGuid():N}"[..20];
        await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Settings Club", slug));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tenant/settings");
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", slug);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Settings Club");
        body.AssociationLatitude.Should().Be(-23.5505);
        body.AssociationLongitude.Should().Be(-46.6333);
    }

    [Fact]
    public async Task PUT_TenantSettings_AsOwner_ShouldReturn200()
    {
        var slug = $"settings-upd-{Guid.NewGuid():N}"[..20];
        await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Old Club", slug));

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/tenant/settings")
        {
            Content = BuildTenantSettingsUpdateContent("Clube Atualizado"),
        };
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", slug);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TenantResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Clube Atualizado");
        body.PlayersPerTeam.Should().Be(11);
        body.LogoPath.Should().NotBeNullOrWhiteSpace();
        body.LogoPath.Should().StartWith("https://res.cloudinary.com/");
        body.City.Should().Be("Rio de Janeiro");
        body.AssociationLatitude.Should().Be(-22.9068);
        body.AssociationLongitude.Should().Be(-43.1729);
    }

    [Fact]
    public async Task PUT_TenantSettings_AsNonOwner_ShouldReturn403()
    {
        var slug = $"settings-forbid-{Guid.NewGuid():N}"[..20];
        var create = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Owner Club", slug));
        var created = await create.Content.ReadFromJsonAsync<TenantResponse>();

        var nonOwnerId = "member-user-id";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByIdAsync(nonOwnerId) is null)
            {
                var user = new ApplicationUser
                {
                    Id = nonOwnerId,
                    UserName = "member@tenant.com",
                    Email = "member@tenant.com",
                    EmailConfirmed = true,
                    IsActive = true,
                };
                var identityResult = await userManager.CreateAsync(user, "Member@123456");
                identityResult.Succeeded.Should().BeTrue();
            }

            db.UserTenants.Add(new UserTenant
            {
                UserId = nonOwnerId,
                TenantId = created!.Id,
                IsOwner = false,
                JoinedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/tenant/settings")
        {
            Content = BuildTenantSettingsUpdateContent("Tentativa Sem Permissao"),
        };
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", slug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, nonOwnerId);
        request.Headers.Add(TestAuthHandler.UserEmailHeader, "member@tenant.com");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task POST_TenantGameDayOption_AsOwner_ShouldReturn201()
    {
        var slug = $"settings-gdo-{Guid.NewGuid():N}"[..20];
        await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Option Club", slug));

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant/settings/game-day-options")
        {
            Content = JsonContent.Create(new
            {
                dayOfWeek = DayOfWeek.Tuesday,
                localStartTime = new TimeOnly(20, 0),
            }),
        };
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", slug);

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TenantGameDayOptionResponse>();
        body.Should().NotBeNull();
        body!.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        body.LocalStartTime.Should().Be(new TimeOnly(20, 0));
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task POST_TenantGameDayOption_DuplicateSlot_ShouldReturn409()
    {
        var slug = $"settings-gdo-dup-{Guid.NewGuid():N}"[..20];
        await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Option Club", slug));

        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant/settings/game-day-options")
        {
            Content = JsonContent.Create(new
            {
                dayOfWeek = DayOfWeek.Thursday,
                localStartTime = new TimeOnly(19, 30),
            }),
        };
        first.Headers.Authorization = new("Bearer", "test-token");
        first.Headers.Add("X-Tenant-Slug", slug);

        var firstResponse = await _client.SendAsync(first);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var duplicate = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant/settings/game-day-options")
        {
            Content = JsonContent.Create(new
            {
                dayOfWeek = DayOfWeek.Thursday,
                localStartTime = new TimeOnly(19, 30),
            }),
        };
        duplicate.Headers.Authorization = new("Bearer", "test-token");
        duplicate.Headers.Add("X-Tenant-Slug", slug);

        var response = await _client.SendAsync(duplicate);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("TENANT_GAMEDAY_OPTION_ALREADY_EXISTS");
    }

    [Fact]
    public async Task GET_TenantGameDayOptions_AsMember_ShouldReturn200WithList()
    {
        var slug = $"settings-gdo-list-{Guid.NewGuid():N}"[..20];
        await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Option Club", slug));

        using (var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant/settings/game-day-options")
        {
            Content = JsonContent.Create(new
            {
                dayOfWeek = DayOfWeek.Saturday,
                localStartTime = new TimeOnly(9, 0),
            }),
        })
        {
            createRequest.Headers.Authorization = new("Bearer", "test-token");
            createRequest.Headers.Add("X-Tenant-Slug", slug);
            var createResponse = await _client.SendAsync(createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        using var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tenant/settings/game-day-options");
        listRequest.Headers.Authorization = new("Bearer", "test-token");
        listRequest.Headers.Add("X-Tenant-Slug", slug);

        var response = await _client.SendAsync(listRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<TenantGameDayOptionResponse>>();
        body.Should().NotBeNull();
        body.Should().ContainSingle(x => x.DayOfWeek == DayOfWeek.Saturday && x.LocalStartTime == new TimeOnly(9, 0));
    }

    [Fact]
    public async Task PUT_TenantGameDayOptionStatus_AsNonOwner_ShouldReturn403()
    {
        var slug = $"settings-gdo-forbid-{Guid.NewGuid():N}"[..20];
        var create = await _client.PostAsync("/api/v1/tenant", BuildTenantCreateContent("Owner Club", slug));
        var created = await create.Content.ReadFromJsonAsync<TenantResponse>();

        Guid optionId;
        using (var createOptionRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/tenant/settings/game-day-options")
        {
            Content = JsonContent.Create(new
            {
                dayOfWeek = DayOfWeek.Monday,
                localStartTime = new TimeOnly(21, 0),
            }),
        })
        {
            createOptionRequest.Headers.Authorization = new("Bearer", "test-token");
            createOptionRequest.Headers.Add("X-Tenant-Slug", slug);
            var createOptionResponse = await _client.SendAsync(createOptionRequest);
            createOptionResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var option = await createOptionResponse.Content.ReadFromJsonAsync<TenantGameDayOptionResponse>();
            optionId = option!.Id;
        }

        var nonOwnerId = "member-user-id-2";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (await userManager.FindByIdAsync(nonOwnerId) is null)
            {
                var user = new ApplicationUser
                {
                    Id = nonOwnerId,
                    UserName = "member2@tenant.com",
                    Email = "member2@tenant.com",
                    EmailConfirmed = true,
                    IsActive = true,
                };
                var identityResult = await userManager.CreateAsync(user, "Member2@123456");
                identityResult.Succeeded.Should().BeTrue();
            }

            db.UserTenants.Add(new UserTenant
            {
                UserId = nonOwnerId,
                TenantId = created!.Id,
                IsOwner = false,
                JoinedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/tenant/settings/game-day-options/{optionId}/status")
        {
            Content = JsonContent.Create(new
            {
                isActive = false,
            }),
        };
        request.Headers.Authorization = new("Bearer", "test-token");
        request.Headers.Add("X-Tenant-Slug", slug);
        request.Headers.Add(TestAuthHandler.UserIdHeader, nonOwnerId);
        request.Headers.Add(TestAuthHandler.UserEmailHeader, "member2@tenant.com");

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Title.Should().Be("FORBIDDEN");
    }
}

