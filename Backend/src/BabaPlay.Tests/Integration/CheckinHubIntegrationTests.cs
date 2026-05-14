using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BabaPlay.Application.DTOs;
using BabaPlay.Infrastructure.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace BabaPlay.Tests.Integration;

public sealed class CheckinHubIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private readonly PlayerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CheckinHubIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
    }

    [Fact]
    public async Task Hub_CheckinCreated_ShouldBroadcastToGameDayGroup()
    {
        var userId = PlayerWebApplicationFactory.TestUserIds[8];
        var playerId = await CreatePlayerAsync(userId, "Hub Player");
        var gameDayId = await CreateGameDayAsync("Hub Day", DateTime.UtcNow.Date.AddDays(1).AddHours(10));

        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var createdTcs = new TaskCompletionSource<(Guid GameDayId, Guid PlayerId)>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_client.BaseAddress!, "/hubs/checkin"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Headers["X-Tenant-Slug"] = PlayerWebApplicationFactory.TestTenantSlug;
                options.Headers["X-Test-UserId"] = userId.ToString();
            })
            .Build();

        connection.On<JsonElement>(CheckinHub.CheckinCreatedEvent, payload =>
        {
            var payloadGameDayId = payload.GetProperty("gameDayId").GetGuid();
            var payloadPlayerId = payload.GetProperty("playerId").GetGuid();
            createdTcs.TrySetResult((payloadGameDayId, payloadPlayerId));
        });

        await connection.StartAsync();
        await connection.InvokeAsync("JoinGameDay", gameDayId);

        var postResponse = await _client.PostAsync("/api/v1/checkin", JsonContent.Create(new
        {
            playerId,
            gameDayId,
            checkedInAtUtc = DateTime.UtcNow.Date.AddDays(1).AddHours(9),
            latitude = -23.5505,
            longitude = -46.6333,
        }));

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var completedTask = await Task.WhenAny(createdTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completedTask.Should().Be(createdTcs.Task, "hub should broadcast checkinCreated to the joined game day group");

        var payload = await createdTcs.Task;
        payload.GameDayId.Should().Be(gameDayId);
        payload.PlayerId.Should().Be(playerId);

        await connection.DisposeAsync();
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
        var player = await response.Content.ReadFromJsonAsync<PlayerResponse>();
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
            description = "Hub integration",
            maxPlayers = 22,
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var gameDay = await response.Content.ReadFromJsonAsync<GameDayResponse>();
        gameDay.Should().NotBeNull();
        return gameDay!.Id;
    }
}
