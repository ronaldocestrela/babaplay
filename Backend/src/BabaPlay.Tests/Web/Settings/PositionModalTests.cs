using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Settings;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Settings;

public class PositionModalTests : TestContext
{
    [Fact]
    public void PositionModal_WhenIsOpenFalse_ShouldNotRender()
    {
        // Act
        var cut = RenderComponent<PositionModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.FindAll("[data-testid='position-modal']").Should().BeEmpty();
    }

    [Fact]
    public void PositionModal_WhenEditing_ShouldPrepopulatePositionFields()
    {
        // Arrange
        var position = new PositionDto(
            Id: Guid.NewGuid(),
            Code: "GOL",
            Name: "Goleiro",
            Description: "Defensor das redes",
            IsActive: true);

        // Act
        var cut = RenderComponent<PositionModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Position, position));

        // Assert
        cut.Find("[data-testid='position-modal']").Should().NotBeNull();
        cut.Find("[data-testid='position-code-input']").GetAttribute("value").Should().Be("GOL");
        cut.Find("[data-testid='position-name-input']").GetAttribute("value").Should().Be("Goleiro");
    }

    [Fact]
    public void PositionModal_WhenSubmittedWithValidData_ShouldTriggerOnCreate()
    {
        // Arrange
        CreatePositionDto? createdDto = null;

        var cut = RenderComponent<PositionModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.OnCreate, (CreatePositionDto dto) => createdDto = dto));

        // Act
        cut.Find("[data-testid='position-code-input']").Change("ATA");
        cut.Find("[data-testid='position-name-input']").Change("Atacante");

        cut.Find("form").Submit();


        // Assert
        createdDto.Should().NotBeNull();
        createdDto!.Code.Should().Be("ATA");
        createdDto.Name.Should().Be("Atacante");
    }
}
