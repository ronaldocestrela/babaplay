using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BabaPlay.Application.DTOs;
using FluentAssertions;

namespace BabaPlay.Tests.Integration;

public sealed class PositionIntegrationTests : IClassFixture<PlayerWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client;

    private static readonly Guid[] Phase5UserIds =
    [
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000008"),
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-000000000009"),
    ];

    public PositionIntegrationTests(PlayerWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", PlayerWebApplicationFactory.TestTenantSlug);
    }

    private static HttpContent CreatePositionBody(string code = "GK", string name = "Goleiro", string? description = null)
        => JsonContent.Create(new { code, name, description });

    [Fact]
    public async Task Post_ValidRequest_ShouldReturn201WithPosition()
    {
        var response = await _client.PostAsync("/api/v1/position", CreatePositionBody("GK", "Goleiro", "Defende"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);
        body.Should().NotBeNull();
        body!.Code.Should().Be("GK");
        body.Name.Should().Be("Goleiro");
    }

    [Fact]
    public async Task Post_DuplicateCode_ShouldReturn409()
    {
        await _client.PostAsync("/api/v1/position", CreatePositionBody("CM", "Meia", null));

        var response = await _client.PostAsync("/api/v1/position", CreatePositionBody("cm", "Meia 2", null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("POSITION_ALREADY_EXISTS");
    }

    [Fact]
    public async Task Post_EmptyCode_ShouldReturn422()
    {
        var response = await _client.PostAsync("/api/v1/position", CreatePositionBody("", "Goleiro", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("INVALID_CODE");
    }

    [Fact]
    public async Task Put_EmptyName_ShouldReturn422()
    {
        var createdResponse = await _client.PostAsync("/api/v1/position", CreatePositionBody("ATA", "Atacante", null));
        var created = await createdResponse.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);

        var response = await _client.PutAsync(
            $"/api/v1/position/{created!.Id}",
            CreatePositionBody("ATA", "", null));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("INVALID_NAME");
    }

    [Fact]
    public async Task GetAll_ShouldReturn200WithList()
    {
        await _client.PostAsync("/api/v1/position", CreatePositionBody("LD", "Lateral", null));

        var response = await _client.GetAsync("/api/v1/position");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<PositionResponse>>(JsonOptions);
        body.Should().NotBeNull();
        body!.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Put_ExistingPosition_ShouldReturn200()
    {
        var createdResponse = await _client.PostAsync("/api/v1/position", CreatePositionBody("VOL", "Volante", null));
        var created = await createdResponse.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);

        var response = await _client.PutAsync(
            $"/api/v1/position/{created!.Id}",
            CreatePositionBody("VOL", "Volante Atualizado", "Meio"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);
        body!.Name.Should().Be("Volante Atualizado");
    }

    [Fact]
    public async Task Delete_ExistingPosition_ShouldReturn204()
    {
        var createdResponse = await _client.PostAsync("/api/v1/position", CreatePositionBody("PE", "Ponta", null));
        var created = await createdResponse.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);

        var response = await _client.DeleteAsync($"/api/v1/position/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_PositionInUse_ShouldReturn409()
    {
        var playerResponse = await _client.PostAsJsonAsync("/api/v1/player", new
        {
            userId = Phase5UserIds[0],
            name = "Position In Use Player",
            nickname = (string?)null,
            phone = (string?)null,
            dateOfBirth = (string?)null,
        });
        playerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var player = await playerResponse.Content.ReadFromJsonAsync<PlayerResponse>(JsonOptions);

        var createdResponse = await _client.PostAsync("/api/v1/position", CreatePositionBody("PIU", "Pos In Use", null));
        var created = await createdResponse.Content.ReadFromJsonAsync<PositionResponse>(JsonOptions);

        var setResponse = await _client.PutAsJsonAsync(
            $"/api/v1/player/{player!.Id}/positions",
            new { positionIds = new[] { created!.Id } });
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/v1/position/{created.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problem = await deleteResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        problem.GetProperty("title").GetString().Should().Be("POSITION_IN_USE");
    }
}
