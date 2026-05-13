using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BabaPlay.Application.DTOs;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class CheckinIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _client;

    public CheckinIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[0]);
    }

    [Fact]
    public async Task Post_ValidCheckin_ShouldReturn201()
    {
        var playerId = await CreatePlayerAsync(PlayerWebApplicationFactory.TestUserIds[0], "Checkin Player 1");
        var gameDayId = await CreateGameDayAsync("Checkin Day 1", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[0]);

        var response = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CheckinResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.PlayerId.Should().Be(playerId);
        body.GameDayId.Should().Be(gameDayId);
    }

    [Fact]
    public async Task Post_OutsideRadius_ShouldReturn422()
    {
        var playerId = await CreatePlayerAsync(PlayerWebApplicationFactory.TestUserIds[1], "Checkin Player 2");
        var gameDayId = await CreateGameDayAsync("Checkin Day 2", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[1]);

        var response = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5610,
            longitude = -46.7000,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("CHECKIN_OUTSIDE_ALLOWED_RADIUS");
    }

    [Fact]
    public async Task Post_DuplicateCheckin_ShouldReturn409()
    {
        var playerId = await CreatePlayerAsync(PlayerWebApplicationFactory.TestUserIds[2], "Checkin Player 3");
        var gameDayId = await CreateGameDayAsync("Checkin Day 3", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[2]);

        await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        var duplicateResponse = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9).AddMinutes(1),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await duplicateResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("CHECKIN_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Post_PlayerInactive_ShouldReturn422()
    {
        var playerId = await CreatePlayerAsync(PlayerWebApplicationFactory.TestUserIds[3], "Checkin Player 4");
        var gameDayId = await CreateGameDayAsync("Checkin Day 4", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[3]);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/player/{playerId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("PLAYER_INACTIVE");
    }

    [Fact]
    public async Task Get_ByGameDay_ShouldReturn200WithItems()
    {
        var playerId = await CreatePlayerAsync(PlayerWebApplicationFactory.TestUserIds[4], "Checkin Player 5");
        var gameDayId = await CreateGameDayAsync("Checkin Day 5", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[4]);

        await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        var response = await _client.GetAsync($"/api/v1/checkin/gameday/{gameDayId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<CheckinResponse>>(JsonOptions);
        body.Should().NotBeNull();
        body!.Should().Contain(c => c.GameDayId == gameDayId);
    }

    [Fact]
    public async Task Delete_Checkin_ShouldReturn204AndRemoveFromActiveList()
    {
        var playerId = await CreatePlayerAsync(PlayerWebApplicationFactory.TestUserIds[5], "Checkin Player 6");
        var gameDayId = await CreateGameDayAsync("Checkin Day 6", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(PlayerWebApplicationFactory.TestUserIds[5]);

        var createResponse = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        var created = await createResponse.Content.ReadFromJsonAsync<CheckinResponse>(JsonOptions);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/checkin/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await _client.GetAsync($"/api/v1/checkin/player/{playerId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<List<CheckinResponse>>(JsonOptions);
        list.Should().NotBeNull();
        list.Should().NotContain(c => c.Id == created.Id);
    }

    [Fact]
    public async Task Post_RequesterIsNotPlayerOwner_ShouldReturn403()
    {
        var playerOwnerUserId = PlayerWebApplicationFactory.TestUserIds[6];
        var requesterUserId = PlayerWebApplicationFactory.TestUserIds[7];
        var playerId = await CreatePlayerAsync(playerOwnerUserId, "Checkin Player Owner");
        var gameDayId = await CreateGameDayAsync("Checkin Day Forbidden Create", DateTime.UtcNow.Date.AddDays(1).AddHours(10));
        SetAuthenticatedUser(requesterUserId);

        var response = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Delete_RequesterIsNotPlayerOwner_ShouldReturn403()
    {
        var playerOwnerUserId = PlayerWebApplicationFactory.TestUserIds[8];
        var requesterUserId = PlayerWebApplicationFactory.TestUserIds[9];
        var playerId = await CreatePlayerAsync(playerOwnerUserId, "Checkin Player Owner Delete");
        var gameDayId = await CreateGameDayAsync("Checkin Day Forbidden Delete", DateTime.UtcNow.Date.AddDays(1).AddHours(10));

        SetAuthenticatedUser(playerOwnerUserId);
        var createResponse = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<CheckinResponse>(JsonOptions);
        created.Should().NotBeNull();

        SetAuthenticatedUser(requesterUserId);
        var deleteResponse = await _client.DeleteAsync($"/api/v1/checkin/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await deleteResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("FORBIDDEN");
    }

    private async Task<Guid> CreatePlayerAsync(Guid userId, string name)
    {
        var response = await _client.PostAsync("/api/v1/player", JsonContent.Create(new
        {
            userId,
            name,
            nickname = (string?)null,
            phone = (string?)null,
            dateOfBirth = (DateOnly?)null,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var player = await response.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);
        player.Should().NotBeNull();
        return player!.Id;
    }

    private async Task<Guid> CreateGameDayAsync(string name, DateTime scheduledAt)
    {
        var response = await _client.PostAsync("/api/v1/gameday", JsonContent.Create(new
        {
            name,
            scheduledAt,
            location = "Campo A",
            description = "Checkin integration",
            maxPlayers = 22,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var gameDay = await response.Content.ReadFromJsonAsync<GameDayResponse>(JsonOptions);
        gameDay.Should().NotBeNull();
        return gameDay!.Id;
    }

    private void SetAuthenticatedUser(Guid userId)
    {
        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
    }
}
