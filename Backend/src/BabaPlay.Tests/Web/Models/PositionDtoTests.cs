using System;
using System.Collections.Generic;
using System.Text.Json;
using BabaPlay.Web.Models;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Web.Models;

public class PositionDtoTests
{
    [Fact]
    public void PositionDto_ShouldDeserializeFromJsonList_WithoutException()
    {
        // Arrange
        var json = """
        [
            {
                "id": "e6a127a6-6e27-4a7b-a010-85fef15b8fb9",
                "code": "ATA",
                "name": "Atacante",
                "description": "Atacante principal",
                "isActive": true
            },
            {
                "id": "3b3b4f62-6c2e-4b68-b808-8e6d1b82fb32",
                "code": "ZAG",
                "name": "Zagueiro",
                "description": null,
                "isActive": true
            }
        ]
        """;
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Act
        var result = JsonSerializer.Deserialize<List<PositionDto>>(json, options);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result![0].Code.Should().Be("ATA");
        result[0].Name.Should().Be("Atacante");
        result[1].Code.Should().Be("ZAG");
        result[1].Name.Should().Be("Zagueiro");
    }
}
