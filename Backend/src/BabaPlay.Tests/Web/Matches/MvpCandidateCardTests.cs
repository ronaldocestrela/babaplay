using System;
using Bunit;
using FluentAssertions;
using Xunit;
using BabaPlay.Web.Components.Matches;
using BabaPlay.Web.Models;

namespace BabaPlay.Tests.Web.Matches;

public class MvpCandidateCardTests : TestContext
{
    [Fact]
    public void MvpCandidateCard_WhenUserCandidate_ShouldDisableVoteButtonForSelfVote()
    {
        // Arrange
        var candidate = new MvpCandidateDto(
            Guid.NewGuid(),
            "Rivaldo Santos",
            null,
            "Meia",
            "Time Amarelo",
            2,
            1,
            5,
            50.0,
            IsUserCandidate: true
        );

        // Act
        var cut = RenderComponent<MvpCandidateCard>(parameters => parameters
            .Add(p => p.Candidate, candidate));

        // Assert
        cut.Find("[data-testid='candidate-name']").TextContent.Should().Contain("Rivaldo Santos");
        var voteButton = cut.Find("[data-testid='btn-vote-mvp']");
        voteButton.ClassList.Should().Contain("disabled");
        cut.Markup.Should().Contain("Auto-voto proibido");
    }

    [Fact]
    public void MvpCandidateCard_WhenVoteButtonClicked_ShouldTriggerOnVoteCallback()
    {
        // Arrange
        var candidate = new MvpCandidateDto(
            Guid.NewGuid(),
            "Kaká Silva",
            null,
            "Meia",
            "Time Azul",
            1,
            2,
            8,
            60.0,
            IsUserCandidate: false
        );

        MvpCandidateDto? votedCandidate = null;

        // Act
        var cut = RenderComponent<MvpCandidateCard>(parameters => parameters
            .Add(p => p.Candidate, candidate)
            .Add(p => p.OnVote, (MvpCandidateDto c) => votedCandidate = c));

        var voteButton = cut.Find("[data-testid='btn-vote-mvp']");
        voteButton.Click();

        // Assert
        votedCandidate.Should().NotBeNull();
        votedCandidate!.PlayerId.Should().Be(candidate.PlayerId);
    }
}
