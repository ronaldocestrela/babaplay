using System;
using BabaPlay.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Unit.Domain;

public class PollDomainTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();
    private const string Title = "Escolha da Cor do Novo Uniforme";

    [Fact]
    public void Create_WithValidParameters_ShouldCreateActivePoll()
    {
        var poll = Poll.Create(TenantId, AuthorId, Title, "Votação oficial da associação");

        poll.Should().NotBeNull();
        poll.TenantId.Should().Be(TenantId);
        poll.AuthorId.Should().Be(AuthorId);
        poll.Title.Should().Be(Title);
        poll.Description.Should().Be("Votação oficial da associação");
        poll.IsClosed.Should().BeFalse();
        poll.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AddOption_WithValidText_ShouldAddOption()
    {
        var poll = Poll.Create(TenantId, AuthorId, Title);

        var opt1 = poll.AddOption("Azul e Branco");
        var opt2 = poll.AddOption("Preto e Dourado");

        poll.Options.Should().HaveCount(2);
        opt1.Text.Should().Be("Azul e Branco");
        opt1.Order.Should().Be(1);
        opt2.Text.Should().Be("Preto e Dourado");
        opt2.Order.Should().Be(2);
    }

    [Fact]
    public void Vote_WhenValid_ShouldRegisterVoteAndIncrementOptionCount()
    {
        var poll = Poll.Create(TenantId, AuthorId, Title);
        var opt = poll.AddOption("Opção A");

        var vote = poll.Vote(opt.Id, "user-1");

        poll.Votes.Should().HaveCount(1);
        vote.OptionId.Should().Be(opt.Id);
        vote.UserId.Should().Be("user-1");
        opt.VotesCount.Should().Be(1);
    }

    [Fact]
    public void Vote_WhenUserAlreadyVoted_ShouldThrowInvalidOperationException()
    {
        var poll = Poll.Create(TenantId, AuthorId, Title);
        var opt1 = poll.AddOption("Opção A");
        var opt2 = poll.AddOption("Opção B");

        poll.Vote(opt1.Id, "user-1");

        var act = () => poll.Vote(opt2.Id, "user-1");
        act.Should().Throw<InvalidOperationException>().WithMessage("*already voted*");
    }

    [Fact]
    public void Vote_WhenPollIsClosed_ShouldThrowInvalidOperationException()
    {
        var poll = Poll.Create(TenantId, AuthorId, Title);
        var opt = poll.AddOption("Opção A");
        poll.Close();

        var act = () => poll.Vote(opt.Id, "user-1");
        act.Should().Throw<InvalidOperationException>().WithMessage("*closed*");
    }

    [Fact]
    public void Close_WhenOpen_ShouldClosePoll()
    {
        var poll = Poll.Create(TenantId, AuthorId, Title);

        poll.Close();

        poll.IsClosed.Should().BeTrue();
    }
}
