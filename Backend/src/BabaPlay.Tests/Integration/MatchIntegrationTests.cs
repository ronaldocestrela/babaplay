using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabaPlay.Application.DTOs;
using BabaPlay.Domain.Enums;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class MatchIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public MatchIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
    }

    [Fact]
    public async Task Post_ValidRequest_ShouldReturn201()
    {
        var gameDay = await CreateGameDayAsync("Rodada Match 1");
        var homeTeam = await CreateTeamAsync("Team A");
        var awayTeam = await CreateTeamAsync("Team B");

        var response = await _client.PostAsJsonAsync("/api/v1/match", new
        {
            gameDayId = gameDay.Id,
            homeTeamId = homeTeam.Id,
            awayTeamId = awayTeam.Id,
            description = "Abertura",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MatchResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Status.Should().Be(MatchStatus.Pending);
    }

    [Fact]
    public async Task Post_DuplicateMatch_ShouldReturn409()
    {
        var gameDay = await CreateGameDayAsync("Rodada Match 2");
        var homeTeam = await CreateTeamAsync("Team C");
        var awayTeam = await CreateTeamAsync("Team D");

        await _client.PostAsJsonAsync("/api/v1/match", new
        {
            gameDayId = gameDay.Id,
            homeTeamId = homeTeam.Id,
            awayTeamId = awayTeam.Id,
            description = "Primeiro",
        });

        var response = await _client.PostAsJsonAsync("/api/v1/match", new
        {
            gameDayId = gameDay.Id,
            homeTeamId = awayTeam.Id,
            awayTeamId = homeTeam.Id,
            description = "Duplicado invertido",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("MATCH_ALREADY_EXISTS");
    }

    [Fact]
    public async Task PutStatus_ValidFlow_ShouldReturn200()
    {
        var created = await CreateMatchAsync("Rodada Match 3", "Team E", "Team F");

        var schedule = await _client.PutAsJsonAsync($"/api/v1/match/{created.Id}/status", new { status = MatchStatus.Scheduled });
        schedule.StatusCode.Should().Be(HttpStatusCode.OK);

        var start = await _client.PutAsJsonAsync($"/api/v1/match/{created.Id}/status", new { status = MatchStatus.InProgress });
        start.StatusCode.Should().Be(HttpStatusCode.OK);

        var finish = await _client.PutAsJsonAsync($"/api/v1/match/{created.Id}/status", new { status = MatchStatus.Completed });
        finish.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ExistingMatch_ShouldReturn204()
    {
        var created = await CreateMatchAsync("Rodada Match 4", "Team G", "Team H");

        var response = await _client.DeleteAsync($"/api/v1/match/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<GameDayResponse> CreateGameDayAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/gameday", new
        {
            name,
            scheduledAt = DateTime.UtcNow.AddHours(5),
            location = "Campo",
            description = (string?)null,
            maxPlayers = 22,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);
        body.Should().NotBeNull();
        return body!;
    }

    private async Task<TeamResponse> CreateTeamAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/team", new
        {
            name,
            maxPlayers = 11,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);
        body.Should().NotBeNull();
        return body!;
    }

    private async Task<MatchResponse> CreateMatchAsync(string gameDayName, string homeTeamName, string awayTeamName)
    {
        var gameDay = await CreateGameDayAsync(gameDayName);
        var homeTeam = await CreateTeamAsync(homeTeamName);
        var awayTeam = await CreateTeamAsync(awayTeamName);

        var response = await _client.PostAsJsonAsync("/api/v1/match", new
        {
            gameDayId = gameDay.Id,
            homeTeamId = homeTeam.Id,
            awayTeamId = awayTeam.Id,
            description = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MatchResponse>(JsonOptions);
        body.Should().NotBeNull();
        return body!;
    }
}