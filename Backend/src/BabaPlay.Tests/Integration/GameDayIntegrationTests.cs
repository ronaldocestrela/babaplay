using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabaPlay.Application.DTOs;
using BabaPlay.Domain.Enums;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class GameDayIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public GameDayIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
    }

    private static HttpContent CreateBody(
        string name = "Rodada Domingo",
        DateTime? scheduledAt = null,
        string? location = null,
        string? description = null,
        int maxPlayers = 22)
        => JsonContent.Create(new
        {
            name,
            scheduledAt = scheduledAt ?? DateTime.UtcNow.AddHours(2),
            location,
            description,
            maxPlayers,
        });

    [Fact]
    public async Task Post_ValidRequest_ShouldReturn201WithGameDay()
    {
        var response = await _client.PostAsync(
            "/api/v1/gameday",
            CreateBody("Rodada A", DateTime.UtcNow.AddHours(4), "Campo A", "Jogo semanal", 18));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Rodada A");
        body.Status.Should().Be(GameDayStatus.Pending);
    }

    [Fact]
    public async Task Post_DuplicateNameAndSchedule_ShouldReturn409()
    {
        var scheduledAt = DateTime.UtcNow.AddHours(5);
        await _client.PostAsync("/api/v1/gameday", CreateBody("Rodada B", scheduledAt));

        var response = await _client.PostAsync("/api/v1/gameday", CreateBody("rodada b", scheduledAt));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("GAMEDAY_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Post_PastScheduledAt_ShouldReturn422()
    {
        var response = await _client.PostAsync(
            "/api/v1/gameday",
            CreateBody("Rodada C", DateTime.UtcNow.AddMinutes(-10)));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("INVALID_SCHEDULED_AT");
    }

    [Fact]
    public async Task GetAll_ShouldReturn200WithList()
    {
        await _client.PostAsync("/api/v1/gameday", CreateBody("Rodada D", DateTime.UtcNow.AddHours(6)));

        var response = await _client.GetAsync("/api/v1/gameday");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<GameDayResponse>>(JsonOptions);
        body.Should().NotBeNull();
        body!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PutStatus_ValidTransition_ShouldReturn200()
    {
        var createdResponse = await _client.PostAsync("/api/v1/gameday", CreateBody("Rodada E", DateTime.UtcNow.AddHours(7)));
        var created = await createdResponse.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/gameday/{created!.Id}/status",
            new { status = GameDayStatus.Confirmed });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);
        body!.Status.Should().Be(GameDayStatus.Confirmed);
    }

    [Fact]
    public async Task PutStatus_InvalidTransition_ShouldReturn422()
    {
        var createdResponse = await _client.PostAsync("/api/v1/gameday", CreateBody("Rodada F", DateTime.UtcNow.AddHours(8)));
        var created = await createdResponse.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);

        var cancelResponse = await _client.PutAsJsonAsync(
            $"/api/v1/gameday/{created!.Id}/status",
            new { status = GameDayStatus.Cancelled });
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var invalidResponse = await _client.PutAsJsonAsync(
            $"/api/v1/gameday/{created.Id}/status",
            new { status = GameDayStatus.Confirmed });

        invalidResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await invalidResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task Delete_ExistingGameDay_ShouldReturn204()
    {
        var createdResponse = await _client.PostAsync("/api/v1/gameday", CreateBody("Rodada G", DateTime.UtcNow.AddHours(9)));
        var created = await createdResponse.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);

        var response = await _client.DeleteAsync($"/api/v1/gameday/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
