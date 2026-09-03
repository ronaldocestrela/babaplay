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
    public void MatchModal_WhenPastDateSelected_ShouldAllowSaveAndRenderHistoricalButton()
    {
        // Arrange
        var model = new ScheduleGameDayDto
        {
            Name = "Baba Passado",
            ScheduledAt = DateTime.Now.AddDays(-1),
            Location = "Arena Velha",
            MaxPlayers = 20,
            Status = "Completed"
        };

        bool saved = false;
        ScheduleGameDayDto? submittedDto = null;

        // Act
        var cut = RenderComponent<MatchModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsEditing, false)
            .Add(p => p.Model, model)
            .Add(p => p.OnSave, (ScheduleGameDayDto dto) => { saved = true; submittedDto = dto; }));

        var btn = cut.Find("[data-testid='btn-save-match']");
        btn.TextContent.Should().Contain("Registrar histórico");

        var form = cut.Find("form");
        form.Submit();

        // Assert
        saved.Should().BeTrue();
        submittedDto.Should().NotBeNull();
        submittedDto!.Status.Should().Be("Completed");
    }

    [Fact]
    public void MatchModal_WhenDateOlderThan10Years_ShouldDisplayValidationErrorAlert()
    {
        // Arrange
        var model = new ScheduleGameDayDto
        {
            Name = "Baba Muito Antigo",
            ScheduledAt = DateTime.Now.AddYears(-11),
            Location = "Arena Antiga",
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
        cut.Find("[data-testid='modal-error-alert']").TextContent.Should().Contain("não podem ser anteriores a 10 anos");
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
        submittedDto.Status.Should().Be("Confirmed");
    }

    [Fact]
    public void MatchModal_WhenCreating_ShouldRenderStatusSelect()
    {
        var model = new ScheduleGameDayDto
        {
            Name = "Baba",
            ScheduledAt = DateTime.Now.AddDays(2),
            Location = "Arena",
            MaxPlayers = 20,
            Status = "Pending"
        };

        var cut = RenderComponent<MatchModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsEditing, false)
            .Add(p => p.Model, model));

        cut.Find("[data-testid='input-match-status']").Should().NotBeNull();
        cut.Markup.Should().Contain("Pendente (rascunho)");
        cut.Markup.Should().Contain("Agendado (aberto para confirmações)");
        cut.Markup.Should().Contain("Concluído (já realizado / histórico)");
    }
}
