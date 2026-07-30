using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class MatchModalTests : TestContext
{
    [Fact]
    public void MatchModal_WhenPastDateSelected_ShouldDisplayValidationErrorAlert()
    {
        // Arrange
        var model = new ScheduleGameDayDto
        {
            Name = "Baba Passado",
            ScheduledAt = DateTime.Now.AddDays(-1),
            Location = "Arena Velha",
            MaxPlayers = 20
        };

        bool saved = false;

        // Act
        var cut = RenderComponent<MatchModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsEditing, false)
            .Add(p => p.Model, model)
            .Add(p => p.OnSave, (ScheduleGameDayDto dto) => saved = true));

        var form = cut.Find("form");
        form.Submit();

        // Assert
        saved.Should().BeFalse();
        cut.Find("[data-testid='modal-error-alert']").TextContent.Should().Contain("A data e hora do agendamento devem ser no futuro");
    }

    [Fact]
    public void MatchModal_WhenValidModelSubmitted_ShouldTriggerOnSaveCallback()
    {
        // Arrange
        var model = new ScheduleGameDayDto
        {
            Name = "Baba Futuro",
            ScheduledAt = DateTime.Now.AddDays(2),
            Location = "Arena Nova",
            MaxPlayers = 20
        };

        ScheduleGameDayDto? submittedDto = null;

        // Act
        var cut = RenderComponent<MatchModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsEditing, false)
            .Add(p => p.Model, model)
            .Add(p => p.OnSave, (ScheduleGameDayDto dto) => submittedDto = dto));

        var form = cut.Find("form");
        form.Submit();

        // Assert
        submittedDto.Should().NotBeNull();
        submittedDto!.Name.Should().Be("Baba Futuro");
    }
}
