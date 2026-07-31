using System;
using System.Text.Json;
using BabaPlay.Web.Models;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Web.Matches;

public class GameDayDtoDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Theory]
    [InlineData("0", "Pending")]
    [InlineData("1", "Confirmed")]
    [InlineData("\"Confirmed\"", "Confirmed")]
    [InlineData("\"pending\"", "Pending")]
    public void GameDayDto_ShouldDeserializeStatusFromNumericOrString(string statusJson, string expectedStatus)
    {
        var json = $$"""
        [
          {
            "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            "name": "Baba Terça",
            "scheduledAt": "2026-08-01T20:00:00Z",
            "location": "Campo Central",
            "description": "Jogo semanal",
            "maxPlayers": 20,
            "status": {{statusJson}}
          }
        ]
        """;

        var result = JsonSerializer.Deserialize<IReadOnlyList<GameDayDto>>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Should().ContainSingle();
        result[0].Status.Should().Be(expectedStatus);
        result[0].Name.Should().Be("Baba Terça");
    }

    [Theory]
    [InlineData("0", "Pending")]
    [InlineData("3", "Completed")]
    [InlineData("\"InProgress\"", "InProgress")]
    public void MatchDto_ShouldDeserializeStatusFromNumericOrString(string statusJson, string expectedStatus)
    {
        var json = $$"""
        [
          {
            "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            "gameDayId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
            "gameDayName": "Baba Terça",
            "scheduledAt": "2026-08-01T20:00:00Z",
            "location": "Campo Central",
            "description": null,
            "homeTeamName": null,
            "awayTeamName": null,
            "status": {{statusJson}},
            "maxPlayers": 20,
            "confirmedCount": 0
          }
        ]
        """;

        var result = JsonSerializer.Deserialize<IReadOnlyList<MatchDto>>(json, JsonOptions);

        result.Should().NotBeNull();
        result!.Should().ContainSingle();
        result[0].Status.Should().Be(expectedStatus);
    }
}
