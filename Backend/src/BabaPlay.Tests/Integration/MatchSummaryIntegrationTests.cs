using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabaPlay.Application.DTOs;
using BabaPlay.Domain.Enums;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class MatchSummaryIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public MatchSummaryIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, PlayerWebApplicationFactory.TestUserIds[0].ToString());
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserEmailHeader, "player-test-1@babaplay.com");
    }

    [Fact]
    public async Task Post_CompletedMatch_ShouldReturn201()
    {
        var match = await CreateCompletedMatchAsync("MS Rodada 1", "MS Team A", "MS Team B");

        var response = await _client.PostAsJsonAsync("/api/v1/match-summary", new
        {
            matchId = match.Id,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MatchSummaryResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.MatchId.Should().Be(match.Id);
    }

    [Fact]
    public async Task GetByMatch_ExistingSummary_ShouldReturn200()
    {
        var match = await CreateCompletedMatchAsync("MS Rodada 2", "MS Team C", "MS Team D");

        await _client.PostAsJsonAsync("/api/v1/match-summary", new { matchId = match.Id });

        var response = await _client.GetAsync($"/api/v1/match-summary/match/{match.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MatchSummaryResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.MatchId.Should().Be(match.Id);
    }

    [Fact]
    public async Task Post_DuplicateSummary_ShouldReturn409()
    {
        var match = await CreateCompletedMatchAsync("MS Rodada 3", "MS Team E", "MS Team F");

        await _client.PostAsJsonAsync("/api/v1/match-summary", new { matchId = match.Id });

        var response = await _client.PostAsJsonAsync("/api/v1/match-summary", new { matchId = match.Id });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("MATCH_SUMMARY_ALREADY_EXISTS");
    }

    [Fact]
    public async Task GetFile_ExistingSummary_ShouldReturnPdf()
    {
        var match = await CreateCompletedMatchAsync("MS Rodada 4", "MS Team G", "MS Team H");

        var createResponse = await _client.PostAsJsonAsync("/api/v1/match-summary", new { matchId = match.Id });
        var summary = await createResponse.Content.ReadFromJsonAsync<MatchSummaryResponse>(JsonOptions);

        var response = await _client.GetAsync($"/api/v1/match-summary/{summary!.Id}/file");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Post_MatchNotFound_ShouldReturn404()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/match-summary", new
        {
            matchId = Guid.NewGuid(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("MATCH_NOT_FOUND");
    }

    [Fact]
    public async Task Post_MatchNotCompleted_ShouldReturn422()
    {
        var gameDay = await CreateGameDayAsync("MS Rodada Not Completed");
        var homeTeam = await CreateTeamAsync("MS Team NC A");
        var awayTeam = await CreateTeamAsync("MS Team NC B");

        var createResponse = await _client.PostAsJsonAsync("/api/v1/match", new
        {
            gameDayId = gameDay.Id,
            homeTeamId = homeTeam.Id,
            awayTeamId = awayTeam.Id,
            description = "not completed",
        });
        var created = await createResponse.Content.ReadFromJsonAsync<MatchResponse>(JsonOptions);

        var response = await _client.PostAsJsonAsync("/api/v1/match-summary", new
        {
            matchId = created!.Id,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("MATCH_NOT_COMPLETED");
    }

    [Fact]
    public async Task GetByMatch_NotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/v1/match-summary/match/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("MATCH_SUMMARY_NOT_FOUND");
    }

    [Fact]
    public async Task GetFile_NotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/v1/match-summary/{Guid.NewGuid()}/file");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("MATCH_SUMMARY_NOT_FOUND");
    }

    private async Task<MatchResponse> CreateCompletedMatchAsync(string gameDayName, string homeTeamName, string awayTeamName)
    {
        var gameDay = await CreateGameDayAsync(gameDayName);
        var homeTeam = await CreateTeamAsync(homeTeamName);
        var awayTeam = await CreateTeamAsync(awayTeamName);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/match", new
        {
            gameDayId = gameDay.Id,
            homeTeamId = homeTeam.Id,
            awayTeamId = awayTeam.Id,
            description = "summary test",
        });

        var created = await createResponse.Content.ReadFromJsonAsync<MatchResponse>(JsonOptions);

        await _client.PutAsJsonAsync($"/api/v1/match/{created!.Id}/status", new { status = MatchStatus.Scheduled });
        await _client.PutAsJsonAsync($"/api/v1/match/{created.Id}/status", new { status = MatchStatus.InProgress });
        await _client.PutAsJsonAsync($"/api/v1/match/{created.Id}/status", new { status = MatchStatus.Completed });

        var getResponse = await _client.GetAsync($"/api/v1/match/{created.Id}");
        var completed = await getResponse.Content.ReadFromJsonAsync<MatchResponse>(JsonOptions);
        return completed!;
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

        var body = await response.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);
        return body!;
    }

    private async Task<TeamResponse> CreateTeamAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/team", new
        {
            name,
            maxPlayers = 11,
        });

        var body = await response.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);
        return body!;
    }
}
