using System.Net;
using System.Net.Http.Json;
using BabaPlay.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BabaPlay.Tests.Integration;

public class PingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_Ping_ShouldReturnHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/ping");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PingStatusDto>();
        body.Should().NotBeNull();
        body!.Status.Should().Be("healthy");
        body.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task POST_Ping_ValidSender_ShouldReturnPong()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/ping", new { sender = "integration-test" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("pong");
    }

    [Fact]
    public async Task POST_Ping_EmptySender_ShouldReturn422()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/ping", new { sender = "" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
