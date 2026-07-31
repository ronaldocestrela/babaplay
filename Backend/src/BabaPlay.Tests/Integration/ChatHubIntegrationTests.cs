using System.Net;
using System.Net.Http.Json;
using BabaPlay.Application.DTOs;
using BabaPlay.Infrastructure.Hubs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;

namespace BabaPlay.Tests.Integration;

public sealed class ChatHubIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private readonly PlayerWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ChatHubIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
    }

    [Fact]
    public async Task Hub_SendMessage_ShouldBroadcastToTenantGroup()
    {
        var userId = PlayerWebApplicationFactory.TestUserIds[0];
        var tenantId = PlayerWebApplicationFactory.TestTenantId;

        _client.DefaultRequestHeaders.Remove(TestAuthHandler.UserIdHeader);
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());

        var messageTcs = new TaskCompletionSource<ChatMessageResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_client.BaseAddress!, "/hubs/chat"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Headers["X-Tenant-Slug"] = PlayerWebApplicationFactory.TestTenantSlug;
                options.Headers[TestAuthHandler.UserIdHeader] = userId.ToString();
            })
            .Build();

        connection.On<ChatMessageResponse>(ChatHub.ReceiveMessageEvent, message =>
        {
            messageTcs.TrySetResult(message);
        });

        await connection.StartAsync();
        await connection.InvokeAsync("JoinTenantChat", tenantId);

        var postResponse = await _client.PostAsJsonAsync(
            "/api/v1/communication/chat/messages",
            new SendChatMessageRequest("Hello team!"));

        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var completedTask = await Task.WhenAny(messageTcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        completedTask.Should().Be(messageTcs.Task, "hub should broadcast receiveMessage to the joined tenant group");

        var received = await messageTcs.Task;
        received.Content.Should().Be("Hello team!");
        received.TenantId.Should().Be(tenantId);

        await connection.DisposeAsync();
    }
}
