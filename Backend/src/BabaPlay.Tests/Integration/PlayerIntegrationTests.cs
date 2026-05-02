using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabaPlay.Application.DTOs;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

/// <summary>
/// Integration tests for <c>PlayerController</c>.
/// Uses <see cref="PlayerWebApplicationFactory"/> with SQLite in-memory databases.
/// </summary>
public sealed class PlayerIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    public PlayerIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static HttpContent CreatePlayerBody(
        Guid? userId = null,
        string name = "Test Player",
        string? nickname = null,
        string? phone = null,
        string? dateOfBirth = null)
    {
        var obj = new
        {
            userId = userId ?? PlayerWebApplicationFactory.TestUserIds[0],
            name,
            nickname,
            phone,
            dateOfBirth,
        };
        return JsonContent.Create(obj);
    }

    private async Task<PlayerResponse> CreateValidPlayerAsync(Guid userId, string name = "Integration Player")
    {
        var response = await _client.PostAsync("/api/v1/player",
            CreatePlayerBody(userId: userId, name: name));
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"because creating player for {userId} should succeed");
        var body = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        return body!;
    }

    private async Task<Guid> CreateValidPositionAsync(string code, string name)
    {
        var response = await _client.PostAsync("/api/v1/position", JsonContent.Create(new
        {
            code,
            name,
            description = (string?)null,
        }));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);
        return body!.Id;
    }

    // ── POST /api/v1/player ──────────────────────────────────────────────────

    [Fact]
    public async Task Post_ValidRequest_ShouldReturn201WithPlayerResponse()
    {
        // User index 0 — dedicated to this test
        var response = await _client.PostAsync("/api/v1/player",
            CreatePlayerBody(
                userId: PlayerWebApplicationFactory.TestUserIds[0],
                name: "Post Valid Player",
                nickname: "PVP",
                phone: "11999991111",
                dateOfBirth: "1990-06-15"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Post Valid Player");
        body.Nickname.Should().Be("PVP");
        body.Phone.Should().Be("11999991111");
        body.IsActive.Should().BeTrue();
        body.Id.Should().NotBeEmpty();
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_DuplicateUser_ShouldReturn409()
    {
        // User index 4 — created twice
        var userId = PlayerWebApplicationFactory.TestUserIds[4];
        await _client.PostAsync("/api/v1/player", CreatePlayerBody(userId: userId, name: "First"));

        var response = await _client.PostAsync("/api/v1/player",
            CreatePlayerBody(userId: userId, name: "Second"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("PLAYER_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Post_UnknownUser_ShouldReturn404()
    {
        var response = await _client.PostAsync("/api/v1/player",
            CreatePlayerBody(userId: Guid.NewGuid(), name: "Ghost Player"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task Post_EmptyName_ShouldReturn422()
    {
        var response = await _client.PostAsync("/api/v1/player",
            CreatePlayerBody(name: ""));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("INVALID_NAME");
    }

    // ── GET /api/v1/player ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturn200WithList()
    {
        var response = await _client.GetAsync("/api/v1/player");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PlayerResponse>>(JsonOptions);
        body.Should().NotBeNull();
    }

    // ── GET /api/v1/player/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingPlayer_ShouldReturn200()
    {
        // User index 1 — dedicated to this test
        var created = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[1], "GetById Player");

        var response = await _client.GetAsync($"/api/v1/player/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        body!.Id.Should().Be(created.Id);
        body.Name.Should().Be("GetById Player");
    }

    [Fact]
    public async Task GetById_UnknownId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/v1/player/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("PLAYER_NOT_FOUND");
    }

    // ── PUT /api/v1/player/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task Put_ExistingPlayer_ShouldReturn200WithUpdatedData()
    {
        // User index 2 — dedicated to this test
        var created = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[2], "Update Target Player");

        var updateBody = JsonContent.Create(new
        {
            name = "Updated Name",
            nickname = "UPD",
            phone = "11988887777",
            dateOfBirth = (string?)null,
        });

        var response = await _client.PutAsync($"/api/v1/player/{created.Id}", updateBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        body!.Name.Should().Be("Updated Name");
        body.Nickname.Should().Be("UPD");
        body.Phone.Should().Be("11988887777");
        body.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Put_UnknownId_ShouldReturn404()
    {
        var updateBody = JsonContent.Create(new
        {
            name = "Does Not Matter",
            nickname = (string?)null,
            phone = (string?)null,
            dateOfBirth = (string?)null,
        });

        var response = await _client.PutAsync($"/api/v1/player/{Guid.NewGuid()}", updateBody);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /api/v1/player/{id} ───────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingPlayer_ShouldReturn204()
    {
        // User index 3 — dedicated to this test
        var created = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[3], "Delete Target Player");

        var response = await _client.DeleteAsync($"/api/v1/player/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_UnknownId_ShouldReturn404()
    {
        var response = await _client.DeleteAsync($"/api/v1/player/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PUT /api/v1/player/{id}/positions ──────────────────────────────────

    [Fact]
    public async Task PutPositions_ValidRequest_ShouldReturn200WithPositions()
    {
        var player = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[5],
            "Player With Positions");
        var gkId = await CreateValidPositionAsync("GK-INTEG", "Goleiro Integ");
        var cmId = await CreateValidPositionAsync("CM-INTEG", "Meia Integ");

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/player/{player.Id}/positions",
            new { positionIds = new[] { gkId, cmId } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PlayerPositionsResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.PositionIds.Should().BeEquivalentTo(new[] { gkId, cmId });
    }

    [Fact]
    public async Task PutPositions_MoreThanThree_ShouldReturn422()
    {
        var player = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[6],
            "Player With Too Many Positions");

        var p1 = await CreateValidPositionAsync("P1-INTEG", "Pos 1");
        var p2 = await CreateValidPositionAsync("P2-INTEG", "Pos 2");
        var p3 = await CreateValidPositionAsync("P3-INTEG", "Pos 3");
        var p4 = await CreateValidPositionAsync("P4-INTEG", "Pos 4");

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/player/{player.Id}/positions",
            new { positionIds = new[] { p1, p2, p3, p4 } });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("POSITIONS_LIMIT_EXCEEDED");
    }

    [Fact]
    public async Task PutPositions_DuplicatePositionIds_ShouldReturn422()
    {
        var player = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[8],
            "Player With Duplicate Positions");

        var repeated = await CreateValidPositionAsync("DUP-INTEG", "Duplicada");

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/player/{player.Id}/positions",
            new { positionIds = new[] { repeated, repeated } });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("DUPLICATE_POSITIONS");
    }

    [Fact]
    public async Task PutPositions_EmptyPositionId_ShouldReturn422()
    {
        var player = await CreateValidPlayerAsync(
            PlayerWebApplicationFactory.TestUserIds[7],
            "Player With Empty Position Id");

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/player/{player.Id}/positions",
            new { positionIds = new[] { Guid.Empty } });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("INVALID_POSITION_ID");
    }
}
