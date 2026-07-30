using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Xunit;
using BabaPlay.Web.Components.Communication;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Communication;

public class PollCardTests : TestContext
{
    public PollCardTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("Test User");
    }

    [Fact]
    public void PollCard_WhenNotVotedAndOpen_ShouldRenderVoteFormOptions()
    {
        // Arrange
        var poll = new PollDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Diretoria",
            Title: "Qual a cor do novo manto?",
            Description: "Votação oficial para 2026",
            ExpiresAtUtc: null,
            IsClosed: false,
            HasVoted: false,
            VotedOptionId: null,
            TotalVotes: 0,
            Options:
            [
                new(Guid.NewGuid(), "Azul e Branco", 1, 0, 0.0),
                new(Guid.NewGuid(), "Preto e Dourado", 2, 0, 0.0)
            ],
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var cut = RenderComponent<PollCard>(parameters => parameters
            .Add(p => p.Poll, poll));

        // Assert
        cut.Find("[data-testid='poll-title']").TextContent.Should().Contain("Qual a cor do novo manto?");
        cut.Find("[data-testid='badge-open']").TextContent.Should().Contain("Enquete Aberta");
        cut.FindAll("[data-testid='radio-option']").Should().HaveCount(2);
        cut.Find("[data-testid='btn-submit-vote']").Should().NotBeNull();
    }

    [Fact]
    public void PollCard_WhenVoted_ShouldRenderResultsProgressBars()
    {
        // Arrange
        var opt1Id = Guid.NewGuid();
        var opt2Id = Guid.NewGuid();

        var poll = new PollDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Diretoria",
            Title: "Local do Baba",
            Description: null,
            ExpiresAtUtc: null,
            IsClosed: false,
            HasVoted: true,
            VotedOptionId: opt1Id,
            TotalVotes: 10,
            Options:
            [
                new(opt1Id, "Campo Principal", 1, 7, 70.0),
                new(opt2Id, "Campo Secundário", 2, 3, 30.0)
            ],
            CreatedAt: DateTime.UtcNow
        );

        // Act
        var cut = RenderComponent<PollCard>(parameters => parameters
            .Add(p => p.Poll, poll));

        // Assert
        cut.Find("[data-testid='badge-voted']").TextContent.Should().Contain("Voto Computado");
        cut.FindAll("[data-testid='result-row']").Should().HaveCount(2);
        cut.Find("[data-testid='badge-my-choice']").TextContent.Should().Contain("Seu voto");
    }

    [Fact]
    public async Task PollCard_ClickOptionAndSubmit_ShouldTriggerOnVoteSubmitted()
    {
        // Arrange
        var opt1Id = Guid.NewGuid();
        var opt2Id = Guid.NewGuid();

        var poll = new PollDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AuthorId: Guid.NewGuid(),
            AuthorName: "Diretoria",
            Title: "Opção preferida?",
            Description: null,
            ExpiresAtUtc: null,
            IsClosed: false,
            HasVoted: false,
            VotedOptionId: null,
            TotalVotes: 0,
            Options:
            [
                new(opt1Id, "Opção A", 1, 0, 0.0),
                new(opt2Id, "Opção B", 2, 0, 0.0)
            ],
            CreatedAt: DateTime.UtcNow
        );

        Guid? votedOptionId = null;

        var cut = RenderComponent<PollCard>(parameters => parameters
            .Add(p => p.Poll, poll)
            .Add(p => p.OnVoteSubmitted, (Guid id) => votedOptionId = id));

        // Select option 1
        var radio = cut.FindAll("[data-testid='radio-option']")[0];
        await radio.ChangeAsync(new ChangeEventArgs { Value = opt1Id });

        // Submit
        var submitBtn = cut.Find("[data-testid='btn-submit-vote']");
        await submitBtn.ClickAsync(new MouseEventArgs());

        // Assert
        votedOptionId.Should().Be(opt1Id);
    }
}
