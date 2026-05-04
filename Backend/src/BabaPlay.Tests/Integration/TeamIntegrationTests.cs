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
        var teamResponse = await _client.PostAsJsonAsync("/api/v1/team", new { name = "Limit Team", maxPlayers = 1 });
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);

        var player1 = await CreatePlayerAsync(TeamUserIds[0], "Player A");
        var player2 = await CreatePlayerAsync(TeamUserIds[1], "Player B");

        var response = await _client.PutAsJsonAsync($"/api/v1/team/{team!.Id}/players", new
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
        var teamResponse = await _client.PostAsJsonAsync("/api/v1/team", new { name = "No Goalkeeper Team", maxPlayers = 11 });
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);

        var positionResponse = await _client.PostAsJsonAsync("/api/v1/position", new { code = "ATA", name = "Atacante", description = (string?)null });
        var position = await positionResponse.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);

        var player = await CreatePlayerAsync(TeamUserIds[2], "Forward");
        var setPositions = await _client.PutAsJsonAsync($"/api/v1/player/{player.Id}/positions", new
        {
            positionIds = new[] { position!.Id },
        });
        setPositions.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/v1/team/{team!.Id}/players", new
        {
            playerIds = new[] { player.Id },
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("TEAM_GOALKEEPER_REQUIRED");
    }

    [Fact]
    public async Task PutPlayers_WithGoalkeeper_ShouldReturn200()
    {
        var teamResponse = await _client.PostAsJsonAsync("/api/v1/team", new { name = "Goalkeeper Team", maxPlayers = 11 });
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamResponse>(JsonOptions);

        var gkPositionResponse = await _client.PostAsJsonAsync("/api/v1/position", new { code = "GOLEIRO", name = "Goleiro", description = (string?)null });
        var gkPosition = await gkPositionResponse.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);

        var player = await CreatePlayerAsync(TeamUserIds[3], "Goalkeeper");
        var setPositions = await _client.PutAsJsonAsync($"/api/v1/player/{player.Id}/positions", new
        {
            positionIds = new[] { gkPosition!.Id },
        });
        setPositions.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await _client.PutAsJsonAsync($"/api/v1/team/{team!.Id}/players", new
        {
            playerIds = new[] { player.Id },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TeamPlayersResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.PlayerIds.Should().ContainSingle().Which.Should().Be(player.Id);
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