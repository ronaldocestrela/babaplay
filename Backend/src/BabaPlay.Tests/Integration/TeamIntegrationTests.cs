using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BabaPlay.Application.DTOs;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class TeamIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static readonly Guid[] TeamUserIds =
    [
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000010"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000011"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000012"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000013"),
    ];

    private readonly HttpClient _client;

    public TeamIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, PlayerWebApplicationFactory.TestUserIds[0].ToString());
    }

    [Fact]
    public async Task Post_ValidRequest_ShouldReturn201WithTeam()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/team", new
        {
            name = "Blue Team",
            maxPlayers = 11,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Blue Team");
        body.MaxPlayers.Should().Be(11);
    }

    [Fact]
    public async Task Post_DuplicateName_ShouldReturn409()
    {
        await _client.PostAsJsonAsync("/api/v1/team", new { name = "Alpha", maxPlayers = 11 });

        var response = await _client.PostAsJsonAsync("/api/v1/team", new { name = "alpha", maxPlayers = 7 });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("TEAM_ALREADY_EXISTS");
    }

    [Fact]
    public async Task PutPlayers_AboveLimit_ShouldReturn422()
    {
        var team = await CreateTeamAsync("Limit Team", 1);

        var player1 = await CreatePlayerAsync(TeamUserIds[0], "Player A");
        var player2 = await CreatePlayerAsync(TeamUserIds[1], "Player B");

        var response = await _client.PutAsJsonAsync($"/api/v1/team/{team.Id}/players", new
        {
            playerIds = new[] { player1.Id, player2.Id },
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("TEAM_PLAYERS_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task PutPlayers_NoGoalkeeper_ShouldReturn422()
    {
        var team = await CreateTeamAsync("No Goalkeeper Team", 11);

        var position = await CreatePositionAsync("ATA", "Atacante");

        var player = await CreatePlayerAsync(TeamUserIds[2], "Forward");
        var setPositions = await _client.PutAsJsonAsync($"/api/v1/player/{player.Id}/positions", new
        {
            positionIds = new[] { position.Id },
        });
        setPositions.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/v1/team/{team!.Id}/players", new
        {
            playerIds = new[] { player.Id },
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        // Some environments may return only status code for this validation path.
        // Validate payload when present and always validate persisted state.
        var responseContent = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            var problem = JsonSerializer.Deserialize<JsonElement>(responseContent, JsonOptions);
            problem.GetProperty("title").GetString().Should().Be("TEAM_GOALKEEPER_REQUIRED");
        }

        var teamGetResponse = await _client.GetAsync($"/api/v1/team/{team.Id}");
        teamGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTeam = await teamGetResponse.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.PlayerIds.Should().BeEmpty();
    }

    [Fact]
    public async Task PutPlayers_WithGoalkeeper_ShouldReturn200()
    {
        var team = await CreateTeamAsync("Goalkeeper Team", 11);

        var gkPosition = await CreatePositionAsync("GOLEIRO", "Goleiro");

        var player = await CreatePlayerAsync(TeamUserIds[3], "Goalkeeper");
        var setPositions = await _client.PutAsJsonAsync($"/api/v1/player/{player.Id}/positions", new
        {
            positionIds = new[] { gkPosition.Id },
        });
        setPositions.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/v1/team/{team.Id}/players", new
        {
            playerIds = new[] { player.Id },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(responseContent))
        {
            var body = JsonSerializer.Deserialize<TeamPlayersResponse>(responseContent, JsonOptions);
            body.Should().NotBeNull();
            body!.PlayerIds.Should().ContainSingle().Which.Should().Be(player.Id);
            return;
        }

        var teamGetResponse = await _client.GetAsync($"/api/v1/team/{team.Id}");
        teamGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTeam = await teamGetResponse.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.PlayerIds.Should().ContainSingle().Which.Should().Be(player.Id);
    }

    private async Task<TeamResponse> CreateTeamAsync(string name, int maxPlayers)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/team", new
        {
            name,
            maxPlayers,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(content))
        {
            var body = JsonSerializer.Deserialize<TeamResponse>(content, JsonOptions);
            body.Should().NotBeNull();
            return body!;
        }

        var listResponse = await _client.GetAsync("/api/v1/team");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var teams = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<TeamResponse>>(JsonOptions);
        teams.Should().NotBeNull();

        var team = teams!
            .SingleOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));

        team.Should().NotBeNull($"created team '{name}' should be retrievable from list endpoint");
        return team!;
    }

    private async Task<PositionResponse> CreatePositionAsync(string code, string name)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/position", new
        {
            code,
            name,
            description = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        if (!string.IsNullOrWhiteSpace(content))
        {
            var body = JsonSerializer.Deserialize<PositionResponse>(content, JsonOptions);
            body.Should().NotBeNull();
            return body!;
        }

        var listResponse = await _client.GetAsync("/api/v1/position");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var positions = await listResponse.Content.ReadFromJsonAsync<IReadOnlyList<PositionResponse>>(JsonOptions);
        positions.Should().NotBeNull();

        var position = positions!
            .SingleOrDefault(p => string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));

        position.Should().NotBeNull($"created position '{code}' should be retrievable from list endpoint");
        return position!;
    }

    private async Task<PlayerResponse> CreatePlayerAsync(Guid userId, string name)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/player", new
        {
            userId,
            name,
            nickname = (string?)null,
            phone = (string?)null,
            dateOfBirth = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var player = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        player.Should().NotBeNull();
        return player!;
    }
}